using Microsoft.EntityFrameworkCore;
using PrestamosUSC.Core.Data;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Enums;
using PrestamosUSC.Core.Exceptions;
using PrestamosUSC.Core.Extensions;

namespace PrestamosUSC.Core.Services;

/// <summary>
/// Lógica de negocio de los préstamos. Reglas:
///  1. La fecha de fin debe ser posterior a la de inicio.
///  2. No se puede pedir un préstamo en el pasado.
///  3. El recurso debe estar "Disponible" (no en mantenimiento ni fuera de servicio).
///  4. La duración no puede superar el máximo del tipo de recurso (Equipo 7 días, Sala 4 horas).
///  5. Un usuario puede tener como máximo 3 préstamos activos.
///  6. Un recurso no puede tener dos préstamos activos que se traslapen en el tiempo.
/// </summary>
public class PrestamoService : IPrestamoService
{
    private readonly AppDbContext _db;

    public event PrestamoEventHandler? PrestamoRegistrado;
    public event PrestamoEventHandler? PrestamoDevuelto;

    public PrestamoService(AppDbContext db) => _db = db;

    public async Task<Prestamo> RegistrarAsync(string usuarioId, int recursoId, DateTime inicio, DateTime fin, string? observaciones)
    {
        if (fin <= inicio)
            throw new ReglaNegocioException("La fecha de fin debe ser posterior a la fecha de inicio.");

        if (inicio < DateTime.Now.AddMinutes(-ReglasPrestamo.ToleranciaMinutosFechaPasada))
            throw new ReglaNegocioException("No se puede registrar un préstamo en una fecha pasada.");

        var recurso = await _db.Recursos.FindAsync(recursoId)
                      ?? throw new ReglaNegocioException("El recurso seleccionado no existe.");

        if (recurso.Estado != EstadoRecurso.Disponible)
            throw new ReglaNegocioException($"El recurso no está disponible ({recurso.Estado.Texto()}).");

        if (fin - inicio > recurso.DuracionMaximaPrestamo)
            throw new ReglaNegocioException(
                $"La duración máxima para este recurso es de {recurso.DuracionMaximaPrestamo.ToTextoLegible()}.");

        var activosDelUsuario = await _db.Prestamos
            .CountAsync(p => p.UsuarioId == usuarioId && p.Estado == EstadoPrestamo.Activo);
        if (activosDelUsuario >= ReglasPrestamo.MaxPrestamosActivosPorUsuario)
            throw new ReglaNegocioException(
                $"Ya tienes {ReglasPrestamo.MaxPrestamosActivosPorUsuario} préstamos activos. Devuelve uno antes de solicitar otro.");

        var hayTraslape = await _db.Prestamos.AnyAsync(p =>
            p.RecursoId == recursoId &&
            p.Estado == EstadoPrestamo.Activo &&
            p.FechaInicio < fin &&
            p.FechaFin > inicio);
        if (hayTraslape)
            throw new RecursoOcupadoException(recursoId);

        var prestamo = new Prestamo
        {
            RecursoId = recursoId,
            Recurso = recurso,
            UsuarioId = usuarioId,
            FechaInicio = inicio,
            FechaFin = fin,
            Observaciones = observaciones,
            Estado = EstadoPrestamo.Activo
        };

        _db.Prestamos.Add(prestamo);
        await _db.SaveChangesAsync();

        PrestamoRegistrado?.Invoke(prestamo);
        return prestamo;
    }

    public async Task DevolverAsync(int prestamoId, string usuarioId, bool esAdministrador)
    {
        var prestamo = await ObtenerConPermisoAsync(prestamoId, usuarioId, esAdministrador);

        prestamo.Estado = EstadoPrestamo.Devuelto;
        prestamo.FechaDevolucion = DateTime.Now;
        await _db.SaveChangesAsync();

        PrestamoDevuelto?.Invoke(prestamo);
    }

    public async Task CancelarAsync(int prestamoId, string usuarioId, bool esAdministrador)
    {
        var prestamo = await ObtenerConPermisoAsync(prestamoId, usuarioId, esAdministrador);

        if (prestamo.FechaInicio <= DateTime.Now)
            throw new ReglaNegocioException("Solo se pueden cancelar préstamos que aún no han comenzado. Usa \"Devolver\".");

        prestamo.Estado = EstadoPrestamo.Cancelado;
        await _db.SaveChangesAsync();

        PrestamoDevuelto?.Invoke(prestamo); // el recurso queda libre: se avisa a la lista de espera
    }

    public Task<List<Prestamo>> ListarPorUsuarioAsync(string usuarioId) =>
        _db.Prestamos
            .AsNoTracking()
            .Include(p => p.Recurso)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.FechaInicio)
            .ToListAsync();

    public Task<List<Prestamo>> ListarTodosAsync(EstadoPrestamo? estado = null)
    {
        var consulta = _db.Prestamos
            .AsNoTracking()
            .Include(p => p.Recurso)
            .Include(p => p.Usuario)
            .AsQueryable();

        if (estado.HasValue)
            consulta = consulta.Where(p => p.Estado == estado.Value);

        return consulta.OrderByDescending(p => p.FechaInicio).ToListAsync();
    }

    public async Task<ResumenDashboard> ObtenerResumenAsync()
    {
        var ahora = DateTime.Now;

        var totalRecursos = await _db.Recursos.CountAsync();
        var noDisponibles = await _db.Recursos.CountAsync(r => r.Estado != EstadoRecurso.Disponible);
        var activos = await _db.Prestamos.CountAsync(p => p.Estado == EstadoPrestamo.Activo);
        var vencidos = await _db.Prestamos.CountAsync(p => p.Estado == EstadoPrestamo.Activo && p.FechaFin < ahora);

        // Dictionary: conteo de préstamos por estado (clave = estado, valor = cantidad)
        var porEstado = await _db.Prestamos
            .GroupBy(p => p.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.Estado, x => x.Cantidad);

        return new ResumenDashboard(totalRecursos, activos, vencidos, noDisponibles, porEstado);
    }

    private async Task<Prestamo> ObtenerConPermisoAsync(int prestamoId, string usuarioId, bool esAdministrador)
    {
        var prestamo = await _db.Prestamos
                           .Include(p => p.Recurso)
                           .FirstOrDefaultAsync(p => p.Id == prestamoId)
                       ?? throw new ReglaNegocioException("El préstamo no existe.");

        if (prestamo.UsuarioId != usuarioId && !esAdministrador)
            throw new ReglaNegocioException("No tienes permiso para modificar este préstamo.");

        if (prestamo.Estado != EstadoPrestamo.Activo)
            throw new ReglaNegocioException("El préstamo ya no está activo.");

        return prestamo;
    }
}
