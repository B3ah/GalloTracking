using System.Security.Claims;
using GalloTracking.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Services;

public class AccessService(AppDbContext db)
{
    public int? UserId(ClaimsPrincipal user) => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub"), out var id) ? id : null;
    public bool IsManager(ClaimsPrincipal user) => user.IsInRole("Gestor");
    public async Task<bool> CanAccessRouteAsync(int routeId, ClaimsPrincipal user)
    {
        if (IsManager(user)) return true;
        var userId = UserId(user);
        return userId.HasValue && await db.Rotas.Include(x => x.Motorista).AnyAsync(x => x.Id == routeId && x.Motorista.UsuarioId == userId.Value);
    }
}
