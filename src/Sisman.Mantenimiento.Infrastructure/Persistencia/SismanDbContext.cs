using Microsoft.EntityFrameworkCore;
using Sisman.Mantenimiento.Domain;
using Sisman.Mantenimiento.Domain.Entidades;

namespace Sisman.Mantenimiento.Infrastructure.Persistencia;

public class SismanDbContext(DbContextOptions<SismanDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Catalogo> Catalogos => Set<Catalogo>();
    public DbSet<Planta> Plantas => Set<Planta>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Maquina> Maquinas => Set<Maquina>();
    public DbSet<Sistema> Sistemas => Set<Sistema>();
    public DbSet<Equipo> Equipos => Set<Equipo>();
    public DbSet<Correlativo> Correlativos => Set<Correlativo>();
    public DbSet<MantenimientoPreventivo> MantenimientosPreventivos => Set<MantenimientoPreventivo>();
    public DbSet<FechaProgramada> FechasProgramadas => Set<FechaProgramada>();
    public DbSet<OrdenTrabajo> OrdenesTrabajo => Set<OrdenTrabajo>();
    public DbSet<OrdenTrabajoHistorial> OrdenesTrabajoHistorial => Set<OrdenTrabajoHistorial>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("sisman");

        b.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.Property(x => x.NombreUsuario).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.NombreUsuario).IsUnique();
            e.Property(x => x.ClaveHash).HasMaxLength(500).IsRequired();
        });

        b.Entity<Catalogo>(e =>
        {
            e.ToTable("Catalogo");
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.HasIndex(x => new { x.Categoria, x.Nombre }).IsUnique();
        });

        b.Entity<Planta>(e => { e.ToTable("Planta"); e.Property(x => x.Nombre).HasMaxLength(100).IsRequired(); });
        b.Entity<Area>(e =>
        {
            e.ToTable("Area");
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.HasOne<Planta>().WithMany().HasForeignKey(x => x.PlantaId).OnDelete(DeleteBehavior.Restrict);
        });
        b.Entity<Maquina>(e =>
        {
            e.ToTable("Maquina");
            e.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.Codigo).IsUnique();
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.HasOne<Area>().WithMany().HasForeignKey(x => x.AreaId).OnDelete(DeleteBehavior.Restrict);
        });
        b.Entity<Sistema>(e =>
        {
            e.ToTable("Sistema");
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.HasOne<Maquina>().WithMany().HasForeignKey(x => x.MaquinaId).OnDelete(DeleteBehavior.Restrict);
        });
        b.Entity<Equipo>(e =>
        {
            e.ToTable("Equipo");
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.HasOne<Sistema>().WithMany().HasForeignKey(x => x.SistemaId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Correlativo>(e =>
        {
            e.ToTable("Correlativo");
            e.HasKey(x => x.Nombre);
            e.Property(x => x.Nombre).HasMaxLength(30);
        });

        b.Entity<MantenimientoPreventivo>(e =>
        {
            e.ToTable("MantenimientoPreventivo", t =>
            {
                t.HasCheckConstraint("CK_MP_Frecuencia", "[Frecuencia] BETWEEN 1 AND 4");
                t.HasCheckConstraint("CK_MP_ValorFrecuencia", "[ValorFrecuencia] > 0");
                t.HasCheckConstraint("CK_MP_Mes", "[Frecuencia] NOT IN (3, 4) OR [Mes] BETWEEN 1 AND 12");
            });
            e.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
            e.HasIndex(x => x.Codigo).IsUnique();
            e.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
            e.Property(x => x.CreadoPor).HasMaxLength(50).IsRequired();
            e.Property(x => x.ModificadoPor).HasMaxLength(50);
            e.Property(x => x.AprobadoPor).HasMaxLength(50);
            e.Property(x => x.Version).IsRowVersion();
            e.HasOne<Maquina>().WithMany().HasForeignKey(x => x.MaquinaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Sistema>().WithMany().HasForeignKey(x => x.SistemaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Equipo>().WithMany().HasForeignKey(x => x.EquipoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Catalogo>().WithMany().HasForeignKey(x => x.TipoTrabajoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Catalogo>().WithMany().HasForeignKey(x => x.FuenteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Catalogo>().WithMany().HasForeignKey(x => x.OportunidadId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Catalogo>().WithMany().HasForeignKey(x => x.EspecialidadId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Fechas).WithOne(f => f.MantenimientoPreventivo!)
                .HasForeignKey(f => f.MantenimientoPreventivoId).OnDelete(DeleteBehavior.Restrict);
            e.Navigation(x => x.Fechas).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<FechaProgramada>(e =>
        {
            e.ToTable("FechaProgramada", t => t.HasCheckConstraint("CK_Fecha_Rango", "[FechaInicio] <= [FechaFin]"));
            e.Property(x => x.CreadoPor).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.MantenimientoPreventivoId, x.FechaInicio }).IsUnique();
            e.HasIndex(x => x.OrdenTrabajoId).IsUnique().HasFilter("[OrdenTrabajoId] IS NOT NULL");
            e.HasOne(x => x.OrdenTrabajo).WithMany().HasForeignKey(x => x.OrdenTrabajoId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<OrdenTrabajo>(e =>
        {
            e.ToTable("OrdenTrabajo");
            e.HasIndex(x => x.Numero).IsUnique();
            e.HasIndex(x => new { x.Estado, x.Numero }).IsDescending(false, true).HasDatabaseName("IX_OT_Estado_Numero");
            e.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
            e.Property(x => x.CreadoPor).HasMaxLength(50).IsRequired();
            e.Property(x => x.ModificadoPor).HasMaxLength(50);
            e.Property(x => x.Version).IsRowVersion();
            e.HasOne<MantenimientoPreventivo>().WithMany().HasForeignKey(x => x.MantenimientoPreventivoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Maquina>().WithMany().HasForeignKey(x => x.MaquinaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Sistema>().WithMany().HasForeignKey(x => x.SistemaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Equipo>().WithMany().HasForeignKey(x => x.EquipoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Catalogo>().WithMany().HasForeignKey(x => x.TipoTrabajoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Catalogo>().WithMany().HasForeignKey(x => x.FuenteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Catalogo>().WithMany().HasForeignKey(x => x.OportunidadId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Catalogo>().WithMany().HasForeignKey(x => x.EspecialidadId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Historial).WithOne().HasForeignKey(h => h.OrdenTrabajoId).OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Historial).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<OrdenTrabajoHistorial>(e =>
        {
            e.ToTable("OrdenTrabajoHistorial");
            e.Property(x => x.Usuario).HasMaxLength(50).IsRequired();
        });
    }
}
