using Microsoft.AspNetCore.Identity;

namespace PrestamosUSC.Core.Entities;

/// <summary>Usuario de la aplicación: extiende IdentityUser de ASP.NET Core Identity.</summary>
public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Identificacion { get; set; }

    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
