namespace PrestamosUSC.Core.Entities;

public class Sala : Recurso
{
    public int Capacidad { get; set; }
    public int Piso { get; set; }

    public override string Tipo => "Sala";
    public override TimeSpan DuracionMaximaPrestamo => TimeSpan.FromHours(4);

    public override string Resumen() => $"{base.Resumen()} - capacidad {Capacidad}";
}
