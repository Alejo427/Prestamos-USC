using System.ComponentModel.DataAnnotations;
using PrestamosUSC.Core.Enums;

namespace PrestamosUSC.Core.Entities;

/// <summary>
/// Clase abstracta base de todo lo que se puede prestar (herencia + polimorfismo).
/// EF Core la guarda en una sola tabla (TPH) con una columna discriminadora.
/// </summary>
public abstract class Recurso
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    [Required, StringLength(100)]
    public string Ubicacion { get; set; } = string.Empty;

    public EstadoRecurso Estado { get; set; } = EstadoRecurso.Disponible;

    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();

    // Miembros abstractos: cada tipo de recurso define su propia regla (polimorfismo)
    public abstract string Tipo { get; }
    public abstract TimeSpan DuracionMaximaPrestamo { get; }

    public virtual string Resumen() => $"{Codigo} - {Nombre} ({Ubicacion})";
}
