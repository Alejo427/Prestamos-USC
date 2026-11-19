using System.Linq.Expressions;

namespace PrestamosUSC.Core.Repositories;

/// <summary>Repositorio genérico (uso de generics con restricción where T : class).</summary>
public interface IRepositorio<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<List<T>> ListarAsync(Expression<Func<T, bool>>? filtro = null);
    Task AgregarAsync(T entidad);
    Task ActualizarAsync(T entidad);
    Task EliminarAsync(T entidad);
}
