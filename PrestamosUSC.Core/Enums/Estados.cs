using System.ComponentModel.DataAnnotations;

namespace PrestamosUSC.Core.Enums;

public enum EstadoRecurso
{
    [Display(Name = "Disponible")] Disponible = 0,
    [Display(Name = "En mantenimiento")] EnMantenimiento = 1,
    [Display(Name = "Fuera de servicio")] FueraDeServicio = 2
}

public enum EstadoPrestamo
{
    [Display(Name = "Activo")] Activo = 0,
    [Display(Name = "Devuelto")] Devuelto = 1,
    [Display(Name = "Cancelado")] Cancelado = 2
}
