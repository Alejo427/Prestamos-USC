using System.ComponentModel.DataAnnotations;
using PrestamosUSC.Core.Enums;

namespace PrestamosUSC.Core.Entities;

public class Prestamo
{
    public int Id { get; set; }

    public int RecursoId { get; set; }
    public Recurso Recurso { get; set; } = null!;

    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime? FechaDevolucion { get; set; }

    public EstadoPrestamo Estado { get; set; } = EstadoPrestamo.Activo;

    [StringLength(300)]
    public string? Observaciones { get; set; }

    // Propiedad calculada (no se guarda en la base de datos)
    public bool EstaVencido => Estado == EstadoPrestamo.Activo && DateTime.Now > FechaFin;
}
