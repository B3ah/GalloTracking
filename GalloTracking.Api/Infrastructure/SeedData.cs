using GalloTracking.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Infrastructure;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
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
}
