using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrestamosUSC.Core.Entities;

namespace PrestamosUSC.Core.Data;

/// <summary>
/// Contexto de Entity Framework Core. Hereda de IdentityDbContext para incluir
/// las tablas de ASP.NET Core Identity (usuarios, roles) junto con las del dominio.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Recurso> Recursos => Set<Recurso>();
    public DbSet<Equipo> Equipos => Set<Equipo>();
    public DbSet<Sala> Salas => Set<Sala>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Recurso>(e =>
        {
            e.ToTable("Recursos");
            e.HasIndex(r => r.Codigo).IsUnique();
            e.Property(r => r.Estado).HasConversion<string>().HasMaxLength(30);
            // Una sola tabla para Equipo y Sala (Table Per Hierarchy)
            e.HasDiscriminator<string>("TipoRecurso")
                .HasValue<Equipo>("Equipo")
                .HasValue<Sala>("Sala");
        });

        builder.Entity<Prestamo>(e =>
        {
            e.ToTable("Prestamos");
            e.Property(p => p.Estado).HasConversion<string>().HasMaxLength(30);
            e.HasOne(p => p.Recurso)
                .WithMany(r => r.Prestamos)
                .HasForeignKey(p => p.RecursoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Usuario)
                .WithMany(u => u.Prestamos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => new { p.RecursoId, p.FechaInicio, p.FechaFin });
        });
    }
}
