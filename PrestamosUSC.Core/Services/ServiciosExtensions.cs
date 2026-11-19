using Microsoft.Extensions.DependencyInjection;
using PrestamosUSC.Core.Data;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Extensions;
using PrestamosUSC.Core.Repositories;
using PrestamosUSC.Core.Structures;

namespace PrestamosUSC.Core.Services;

public static class ServiciosExtensions
{
    /// <summary>
    /// Registra todos los servicios de la biblioteca (inyección de dependencias).
    /// También conecta los eventos del servicio de préstamos con la bitácora y la lista de espera.
    /// La futura aplicación de escritorio puede llamar a este mismo método.
    /// </summary>
    public static IServiceCollection AddServiciosPrestamos(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepositorio<>), typeof(Repositorio<>));
        services.AddScoped<IRecursoService, RecursoService>();

        services.AddSingleton<IHistorialActividad, HistorialActividad>();
        services.AddSingleton<IColaEspera, ColaEspera>();

        services.AddScoped<IPrestamoService>(sp =>
        {
            var historial = sp.GetRequiredService<IHistorialActividad>();
            var cola = sp.GetRequiredService<IColaEspera>();
            var servicio = new PrestamoService(sp.GetRequiredService<AppDbContext>());

            servicio.PrestamoRegistrado += p =>
                historial.Registrar($"Préstamo #{p.Id} registrado: {p.Recurso.Codigo} del {p.FechaInicio.ToTextoCorto()} al {p.FechaFin.ToTextoCorto()}");

            servicio.PrestamoDevuelto += p =>
            {
                historial.Registrar($"Préstamo #{p.Id} cerrado ({p.Estado.Texto()}): {p.Recurso.Codigo} queda libre");

                var siguiente = cola.Siguiente(p.RecursoId);
                if (siguiente != null)
                    historial.Registrar($"Avisar a {siguiente.NombreUsuario}: {p.Recurso.Codigo} ya puede solicitarse (primero en la lista de espera)");
            };

            return servicio;
        });

        return services;
    }
}
