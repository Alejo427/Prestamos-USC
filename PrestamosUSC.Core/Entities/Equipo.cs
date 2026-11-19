namespace PrestamosUSC.Core.Entities;

public class Equipo : Recurso
{
    public string? Marca { get; set; }
    public string? Serial { get; set; }

    public override string Tipo => "Equipo";
    public override TimeSpan DuracionMaximaPrestamo => TimeSpan.FromDays(7);

    public override string Resumen() =>
        $"{base.Resumen()} - {(string.IsNullOrWhiteSpace(Marca) ? "sin marca" : Marca)}";
}
