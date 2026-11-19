using System.ComponentModel.DataAnnotations;
using PrestamosUSC.Core.Enums;
using PrestamosUSC.Core.Structures;

namespace PrestamosUSC.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo no válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Identificación")]
    public string? Identificacion { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo no válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RecursoFormViewModel
{
    public int Id { get; set; }

    [Required]
    public string Tipo { get; set; } = "Equipo"; // "Equipo" o "Sala"

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(20)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La ubicación es obligatoria.")]
    [StringLength(100)]
    [Display(Name = "Ubicación")]
    public string Ubicacion { get; set; } = string.Empty;

    public EstadoRecurso Estado { get; set; } = EstadoRecurso.Disponible;

    // Solo Equipo
    public string? Marca { get; set; }
    public string? Serial { get; set; }

    // Solo Sala
    [Range(0, 1000, ErrorMessage = "Capacidad entre 0 y 1000.")]
    public int Capacidad { get; set; }
    [Range(0, 50, ErrorMessage = "Piso entre 0 y 50.")]
    public int Piso { get; set; }
}

public class PrestamoCreateViewModel
{
    public int RecursoId { get; set; }

    // Solo para mostrar (no se envía en el formulario)
    public string RecursoResumen { get; set; } = string.Empty;
    public string DuracionMaxima { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indica la fecha y hora de inicio.")]
    [Display(Name = "Inicio")]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "Indica la fecha y hora de fin.")]
    [Display(Name = "Fin")]
    public DateTime FechaFin { get; set; }

    [StringLength(300)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }
}

public class UsuarioViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EsAdministrador { get; set; }
}

public class EstructurasViewModel
{
    public List<ResultadoComparacion> Resultados { get; set; } = new();
    public IReadOnlyList<string> Historial { get; set; } = new List<string>();
    public IReadOnlyList<SolicitudEspera> ColaEspera { get; set; } = new List<SolicitudEspera>();
}
