using GalloTracking.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Infrastructure;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var environment = services.GetRequiredService<IHostEnvironment>();

        if (environment.IsDevelopment() && db.Database.IsSqlite() && await IsLegacyDatabaseAsync(db))
            await db.Database.EnsureDeletedAsync();

        await db.Database.MigrateAsync();
        if (await db.Usuarios.AnyAsync()) return;
        var hasher = new PasswordHasher<Usuario>();
        var gestor = new Usuario { Nome = "Gestor Demo", Email = "gestor@gallo.local", Perfil = PerfilUsuario.Gestor };
        gestor.SenhaHash = hasher.HashPassword(gestor, "gallo123");
        var motoristaUsuario = new Usuario { Nome = "Motorista Demo", Email = "motorista@gallo.local", Perfil = PerfilUsuario.Motorista };
        motoristaUsuario.SenhaHash = hasher.HashPassword(motoristaUsuario, "gallo123");
        var motorista = new Motorista { Usuario = motoristaUsuario, Telefone = "(14) 99999-0000" };
        var rota = new Rota { Motorista = motorista, Status = StatusRota.Planejada };
        rota.Entregas.Add(new Entrega { Destinatario = "Cliente Demo", Endereco = "Av. Brasil, 100", Status = StatusEntrega.Pendente });
        db.AddRange(gestor, motoristaUsuario, motorista, rota);
        await db.SaveChangesAsync();
    }

    private static async Task<bool> IsLegacyDatabaseAsync(AppDbContext db)
    {
        await using var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name IN ('Usuarios', '__EFMigrationsHistory')";
        var tables = Convert.ToInt32(await command.ExecuteScalarAsync());
        return tables == 1;
    }
}
