using PrestamosUSC.Core.Entities;

namespace PrestamosUSC.Core.Services;

public interface IRecursoService
{
    Task<List<Recurso>> ListarAsync(string? tipo = null, string? busqueda = null, DateTime? desde = null, DateTime? hasta = null);
    Task<Recurso?> ObtenerAsync(int id);
    Task CrearAsync(Recurso recurso);
    Task ActualizarAsync(Recurso recurso);
    Task EliminarAsync(int id);
}
