using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PrestamosUSC.Core.Constants;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Exceptions;
using PrestamosUSC.Core.Extensions;
using PrestamosUSC.Core.Services;

namespace PrestamosUSC.Desktop;

public partial class Form1 : Form
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TextBox _correo = new() { Width = 280, PlaceholderText = "Correo" };
    private readonly TextBox _clave = new() { Width = 280, PlaceholderText = "Contraseña", UseSystemPasswordChar = true };
    private readonly Button _ingresar = new() { Text = "Iniciar sesión", AutoSize = true };
    private readonly Label _sesion = new() { AutoSize = true };
    private readonly Label _estado = new() { Dock = DockStyle.Bottom, Height = 28 };
    private readonly TabControl _pestanas = new() { Dock = DockStyle.Fill };
    private readonly TabPage _paginaPrestamos = new("Mis préstamos");
    private readonly DataGridView _recursos = CrearTabla();
    private readonly DataGridView _prestamos = CrearTabla();
    private readonly FlowLayoutPanel _acceso = new()
    {
        Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20)
    };
    private readonly Panel _panel = new() { Dock = DockStyle.Fill, Visible = false };

    private string? _usuarioId;
    private bool _esAdministrador;
    private bool _ocupado;

    public Form1(IServiceScopeFactory scopeFactory)
    {
        InitializeComponent();
        _scopeFactory = scopeFactory;

        Text = "Préstamos USC";
        ClientSize = new Size(950, 550);
        MinimumSize = new Size(760, 420);
        StartPosition = FormStartPosition.CenterScreen;

        _acceso.Controls.AddRange(new Control[] { _correo, _clave, _ingresar });
        _ingresar.Click += async (_, _) => await EjecutarAsync(IniciarSesionAsync);
        AcceptButton = _ingresar;

        var encabezado = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
        encabezado.Controls.Add(_sesion);
        encabezado.Controls.Add(CrearBoton("Cerrar sesión", () => { CerrarSesion(); return Task.CompletedTask; }));

        var paginaRecursos = new TabPage("Recursos");
        paginaRecursos.Controls.Add(_recursos);
        paginaRecursos.Controls.Add(CrearBarra(
            CrearBoton("Registrar préstamo", async () => await EjecutarAsync(RegistrarAsync)),
            CrearBoton("Actualizar", async () => await EjecutarAsync(CargarDatosAsync))));

        _paginaPrestamos.Controls.Add(_prestamos);
        _paginaPrestamos.Controls.Add(CrearBarra(
            CrearBoton("Devolver", async () => await EjecutarAsync(() => CambiarPrestamoAsync(false))),
            CrearBoton("Cancelar préstamo", async () => await EjecutarAsync(() => CambiarPrestamoAsync(true))),
            CrearBoton("Actualizar", async () => await EjecutarAsync(CargarDatosAsync))));

        _pestanas.TabPages.Add(paginaRecursos);
        _pestanas.TabPages.Add(_paginaPrestamos);
        _panel.Controls.Add(_pestanas);
        _panel.Controls.Add(encabezado);
        Controls.Add(_panel);
        Controls.Add(_acceso);
        Controls.Add(_estado);
    }

    private async Task IniciarSesionAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var usuario = await usuarios.FindByEmailAsync(_correo.Text.Trim());
        if (usuario is null ||
            await usuarios.IsLockedOutAsync(usuario) ||
            await usuarios.GetTwoFactorEnabledAsync(usuario) ||
            !await usuarios.CheckPasswordAsync(usuario, _clave.Text))
        {
            _estado.Text = "No se pudo iniciar sesión. Verifica tus datos o el estado de tu cuenta.";
            return;
        }

        var administrador = await usuarios.IsInRoleAsync(usuario, RolesApp.Administrador);
        await CargarDatosAsync(usuario.Id, administrador);
        _usuarioId = usuario.Id;
        _esAdministrador = administrador;
        _clave.Clear();
        _sesion.Text = usuario.NombreCompleto;
        _paginaPrestamos.Text = administrador ? "Todos los préstamos" : "Mis préstamos";
        _pestanas.SelectedIndex = 0;
        _acceso.Hide();
        _panel.Show();
        AcceptButton = null;
    }

    private void CerrarSesion()
    {
        _usuarioId = null;
        _esAdministrador = false;
        _recursos.DataSource = null;
        _prestamos.DataSource = null;
        _estado.Text = string.Empty;
        _panel.Hide();
        _acceso.Show();
        AcceptButton = _ingresar;
        _correo.Focus();
    }

    private Task CargarDatosAsync() => CargarDatosAsync(_usuarioId!, _esAdministrador);

    private async Task CargarDatosAsync(string usuarioId, bool administrador)
    {
        using var scope = _scopeFactory.CreateScope();
        var recursos = await scope.ServiceProvider.GetRequiredService<IRecursoService>().ListarAsync();
        var servicio = scope.ServiceProvider.GetRequiredService<IPrestamoService>();
        var prestamos = administrador
            ? await servicio.ListarTodosAsync()
            : await servicio.ListarPorUsuarioAsync(usuarioId);

        _recursos.DataSource = recursos.Select(r => new
        {
            r.Id, Código = r.Codigo, r.Nombre, r.Tipo, Ubicación = r.Ubicacion,
            Estado = r.Estado.Texto(), Descripción = r.Descripcion
        }).ToList();
        _prestamos.DataSource = prestamos.Select(p => new
        {
            p.Id, Recurso = p.Recurso.Nombre, Usuario = p.Usuario?.NombreCompleto,
            Inicio = p.FechaInicio.ToString("g"), Fin = p.FechaFin.ToString("g"),
            Estado = p.EstaVencido ? "Vencido" : p.Estado.Texto(), p.Observaciones
        }).ToList();
        if (_prestamos.Columns["Usuario"] is DataGridViewColumn columnaUsuario)
            columnaUsuario.Visible = administrador;
    }

    private async Task RegistrarAsync()
    {
        var fila = _recursos.CurrentRow;
        if (fila?.Cells["Id"].Value is not int id)
        {
            _estado.Text = "Selecciona un recurso en la tabla.";
            return;
        }

        using var dialogo = new SolicitudPrestamoForm(
            Convert.ToString(fila.Cells["Nombre"].Value) ?? "Recurso");
        if (dialogo.ShowDialog(this) != DialogResult.OK)
            return;

        using var scope = _scopeFactory.CreateScope();
        var servicio = scope.ServiceProvider.GetRequiredService<IPrestamoService>();
        await servicio.RegistrarAsync(_usuarioId!, id, dialogo.Inicio, dialogo.Fin, dialogo.Observaciones);
        await ActualizarTrasCambioAsync("Préstamo registrado.");
        _pestanas.SelectedTab = _paginaPrestamos;
    }

    private async Task CambiarPrestamoAsync(bool cancelar)
    {
        if (_prestamos.CurrentRow?.Cells["Id"].Value is not int id)
        {
            _estado.Text = "Selecciona un préstamo en la tabla.";
            return;
        }

        var accion = cancelar ? "cancelar" : "devolver";
        if (MessageBox.Show(this, $"¿Quieres {accion} el préstamo #{id}?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        using var scope = _scopeFactory.CreateScope();
        var servicio = scope.ServiceProvider.GetRequiredService<IPrestamoService>();
        if (cancelar)
            await servicio.CancelarAsync(id, _usuarioId!, _esAdministrador);
        else
            await servicio.DevolverAsync(id, _usuarioId!, _esAdministrador);

        await ActualizarTrasCambioAsync(cancelar ? "Préstamo cancelado." : "Préstamo devuelto.");
    }

    private async Task ActualizarTrasCambioAsync(string mensaje)
    {
        try
        {
            await CargarDatosAsync();
            _estado.Text = mensaje;
        }
        catch (Exception ex)
        {
            _estado.Text = $"{mensaje} Pulsa Actualizar para volver a cargar las tablas.";
            MessageBox.Show(this, ex.Message, "Error al actualizar");
        }
    }

    private async Task EjecutarAsync(Func<Task> tarea)
    {
        if (_ocupado)
            return;

        _ocupado = true;
        _ingresar.Enabled = false;
        _panel.Enabled = false;
        _estado.Text = string.Empty;
        try
        {
            await tarea();
        }
        catch (ReglaNegocioException ex)
        {
            MessageBox.Show(this, ex.Message, "Préstamos USC", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error en Préstamos USC", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _ocupado = false;
            _ingresar.Enabled = true;
            _panel.Enabled = true;
        }
    }

    private static DataGridView CrearTabla() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
        AllowUserToAddRows = false
    };

    private static Button CrearBoton(string texto, Func<Task> accion)
    {
        var boton = new Button { Text = texto, AutoSize = true };
        boton.Click += async (_, _) => await accion();
        return boton;
    }

    private static FlowLayoutPanel CrearBarra(params Control[] controles)
    {
        var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
        barra.Controls.AddRange(controles);
        return barra;
    }
}
