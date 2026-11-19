using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Enums;

namespace PrestamosUSC.Core.Services;

/// <summary>Delegado personalizado usado por los eventos del servicio.</summary>
public delegate void PrestamoEventHandler(Prestamo prestamo);

/// <summary>Resumen para el tablero de inicio (record = tipo inmutable).</summary>
public record ResumenDashboard(
    int TotalRecursos,
    int PrestamosActivos,
    int PrestamosVencidos,
    int RecursosNoDisponibles,
    Dictionary<EstadoPrestamo, int> PrestamosPorEstado);

public interface IPrestamoService
{
    event PrestamoEventHandler? PrestamoRegistrado;
    event PrestamoEventHandler? PrestamoDevuelto;

    Task<Prestamo> RegistrarAsync(string usuarioId, int recursoId, DateTime inicio, DateTime fin, string? observaciones);
    Task DevolverAsync(int prestamoId, string usuarioId, bool esAdministrador);
    Task CancelarAsync(int prestamoId, string usuarioId, bool esAdministrador);
    Task<List<Prestamo>> ListarPorUsuarioAsync(string usuarioId);
    Task<List<Prestamo>> ListarTodosAsync(EstadoPrestamo? estado = null);
    Task<ResumenDashboard> ObtenerResumenAsync();
}
