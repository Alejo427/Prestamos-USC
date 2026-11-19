using PrestamosUSC.Core.Enums;

namespace PrestamosUSC.Core.Extensions;

/// <summary>Métodos de extensión + switch expressions (pattern matching) para presentar estados.</summary>
public static class EstadoExtensions
{
    public static string Texto(this EstadoPrestamo estado) => estado switch
    {
        EstadoPrestamo.Activo => "Activo",
        EstadoPrestamo.Devuelto => "Devuelto",
        EstadoPrestamo.Cancelado => "Cancelado",
        _ => estado.ToString()
    };

    public static string ClaseBadge(this EstadoPrestamo estado) => estado switch
    {
        EstadoPrestamo.Activo => "bg-primary",
        EstadoPrestamo.Devuelto => "bg-success",
        EstadoPrestamo.Cancelado => "bg-secondary",
        _ => "bg-light text-dark"
    };

    public static string Texto(this EstadoRecurso estado) => estado switch
    {
        EstadoRecurso.Disponible => "Disponible",
        EstadoRecurso.EnMantenimiento => "En mantenimiento",
        EstadoRecurso.FueraDeServicio => "Fuera de servicio",
        _ => estado.ToString()
    };

    public static string ClaseBadge(this EstadoRecurso estado) => estado switch
    {
        EstadoRecurso.Disponible => "bg-success",
        EstadoRecurso.EnMantenimiento => "bg-warning text-dark",
        EstadoRecurso.FueraDeServicio => "bg-danger",
        _ => "bg-secondary"
    };
}
