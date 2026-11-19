namespace PrestamosUSC.Core.Extensions;

public static class FechaExtensions
{
    public static string ToTextoCorto(this DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm");

    public static string ToTextoLegible(this TimeSpan duracion) =>
        duracion.TotalDays >= 1
            ? $"{duracion.TotalDays:0.#} día(s)"
            : $"{duracion.TotalHours:0.#} hora(s)";
}
