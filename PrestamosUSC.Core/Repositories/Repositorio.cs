using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PrestamosUSC.Core.Data;

namespace PrestamosUSC.Core.Repositories;

public class Repositorio<T> : IRepositorio<T> where T : class
{
    private readonly AppDbContext _db;
    private readonly DbSet<T> _set;

    public Repositorio(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task<T?> ObtenerPorIdAsync(int id) => await _set.FindAsync(id);

    public async Task<List<T>> ListarAsync(Expression<Func<T, bool>>? filtro = null)
    {
        IQueryable<T> consulta = _set;
        if (filtro != null) consulta = consulta.Where(filtro);
        return await consulta.ToListAsync();
    }

    public async Task AgregarAsync(T entidad)
    {
        await _set.AddAsync(entidad);
        await _db.SaveChangesAsync();
    }

    public async Task ActualizarAsync(T entidad)
    {
        _set.Update(entidad);
        await _db.SaveChangesAsync();
    }

    public async Task EliminarAsync(T entidad)
    {
        _set.Remove(entidad);
        await _db.SaveChangesAsync();
    }
}
