using Microsoft.EntityFrameworkCore;
using PrestamosUSC.Core.Data;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Enums;
using PrestamosUSC.Core.Exceptions;
using PrestamosUSC.Core.Repositories;

namespace PrestamosUSC.Core.Services;

public class RecursoService : IRecursoService
{
    private readonly AppDbContext _db;
    private readonly IRepositorio<Recurso> _repo;

    public RecursoService(AppDbContext db, IRepositorio<Recurso> repo)
    {
        _db = db;
        _repo = repo;
    }

    public async Task<List<Recurso>> ListarAsync(string? tipo = null, string? busqueda = null, DateTime? desde = null, DateTime? hasta = null)
    {
        IQueryable<Recurso> consulta = _db.Recursos.AsNoTracking();

        if (tipo == "Equipo") consulta = consulta.OfType<Equipo>();
        else if (tipo == "Sala") consulta = consulta.OfType<Sala>();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            consulta = consulta.Where(r => r.Nombre.Contains(texto) || r.Codigo.Contains(texto));
        }

        var recursos = await consulta.OrderBy(r => r.Nombre).ToListAsync();

        // Filtro "disponible entre dos fechas": usa un HashSet<int> con los Ids ocupados (búsqueda O(1)).
        if (desde.HasValue && hasta.HasValue && hasta > desde)
        {
            var d = desde.Value;
            var h = hasta.Value;
            var idsOcupados = (await _db.Prestamos
                    .Where(p => p.Estado == EstadoPrestamo.Activo && p.FechaInicio < h && p.FechaFin > d)
                    .Select(p => p.RecursoId)
                    .ToListAsync())
                .ToHashSet();

            recursos = recursos
                .Where(r => r.Estado == EstadoRecurso.Disponible && !idsOcupados.Contains(r.Id))
                .ToList();
        }

        return recursos;
    }

    public Task<Recurso?> ObtenerAsync(int id) => _repo.ObtenerPorIdAsync(id);

    public async Task CrearAsync(Recurso recurso)
    {
        recurso.Codigo = recurso.Codigo.Trim().ToUpperInvariant();
        if (await _db.Recursos.AnyAsync(r => r.Codigo == recurso.Codigo))
            throw new ReglaNegocioException($"Ya existe un recurso con el código {recurso.Codigo}.");

        await _repo.AgregarAsync(recurso);
    }

    public async Task ActualizarAsync(Recurso recurso)
    {
        recurso.Codigo = recurso.Codigo.Trim().ToUpperInvariant();
        if (await _db.Recursos.AnyAsync(r => r.Codigo == recurso.Codigo && r.Id != recurso.Id))
            throw new ReglaNegocioException($"Ya existe otro recurso con el código {recurso.Codigo}.");

        await _repo.ActualizarAsync(recurso);
    }

    public async Task EliminarAsync(int id)
    {
        var recurso = await _repo.ObtenerPorIdAsync(id)
                      ?? throw new ReglaNegocioException("El recurso no existe.");

        if (await _db.Prestamos.AnyAsync(p => p.RecursoId == id))
            throw new ReglaNegocioException("No se puede eliminar: el recurso tiene historial de préstamos. Márcalo como \"Fuera de servicio\".");

        await _repo.EliminarAsync(recurso);
    }
}
