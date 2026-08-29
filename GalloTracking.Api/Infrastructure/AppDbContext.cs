using GalloTracking.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Motorista> Motoristas => Set<Motorista>();
    public DbSet<Rota> Rotas => Set<Rota>();
    public DbSet<Entrega> Entregas => Set<Entrega>();
    public DbSet<Localizacao> Localizacoes => Set<Localizacao>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Usuario>().HasIndex(x => x.Email).IsUnique();
        model.Entity<Usuario>().Property(x => x.Perfil).HasConversion<string>();
        model.Entity<Rota>().Property(x => x.Status).HasConversion<string>();
        model.Entity<Entrega>().Property(x => x.Status).HasConversion<string>();
        model.Entity<Usuario>().HasOne(x => x.Motorista).WithOne(x => x.Usuario).HasForeignKey<Motorista>(x => x.UsuarioId);
        model.Entity<Rota>().HasOne(x => x.Motorista).WithMany(x => x.Rotas).HasForeignKey(x => x.MotoristaId);
        model.Entity<Entrega>().HasOne(x => x.Rota).WithMany(x => x.Entregas).HasForeignKey(x => x.RotaId);
        model.Entity<Localizacao>().HasOne(x => x.Rota).WithMany(x => x.Localizacoes).HasForeignKey(x => x.RotaId);
        model.Entity<Localizacao>().HasIndex(x => new { x.MotoristaId, x.IdLocal }).IsUnique();
    }
}
