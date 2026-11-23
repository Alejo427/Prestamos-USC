namespace PrestamosUSC.Desktop;

internal sealed class SolicitudPrestamoForm : Form
{
    private readonly DateTimePicker _inicio = CrearSelector();
    private readonly DateTimePicker _fin = CrearSelector();
    private readonly TextBox _observaciones = new()
    {
        Width = 330, Height = 60, Multiline = true, MaxLength = 300
    };

    public DateTime Inicio => _inicio.Value;
    public DateTime Fin => _fin.Value;
    public string? Observaciones => string.IsNullOrWhiteSpace(_observaciones.Text)
        ? null
        : _observaciones.Text.Trim();

    public SolicitudPrestamoForm(string recurso)
    {
        Text = $"Registrar préstamo · {recurso}";
        ClientSize = new Size(380, 250);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var ahora = DateTime.Now;
        _inicio.Value = ahora.AddMinutes(10);
        _fin.Value = _inicio.Value.AddHours(1);

        var campos = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(12),
            FlowDirection = FlowDirection.TopDown, WrapContents = false
        };
        var registrar = new Button { Text = "Registrar", DialogResult = DialogResult.OK, AutoSize = true };
        var volver = new Button { Text = "Volver", DialogResult = DialogResult.Cancel, AutoSize = true };
        var botones = new FlowLayoutPanel { AutoSize = true };
        botones.Controls.AddRange(new Control[] { registrar, volver });
        campos.Controls.AddRange(new Control[]
        {
            new Label { Text = "Inicio", AutoSize = true }, _inicio,
            new Label { Text = "Fin", AutoSize = true }, _fin,
            new Label { Text = "Observaciones (opcional)", AutoSize = true }, _observaciones,
            botones
        });
        Controls.Add(campos);
        AcceptButton = registrar;
        CancelButton = volver;
    }

    private static DateTimePicker CrearSelector() => new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd/MM/yyyy HH:mm",
        Width = 330
    };
}
