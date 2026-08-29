using GalloTracking.Api.Contracts;
using GalloTracking.Api.Domain;
using GalloTracking.Api.Hubs;
using GalloTracking.Api.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Services;

public class TrackingService(AppDbContext db, IHubContext<LocalizacaoHub> hub)
{
    public async Task<(Localizacao? Location, string? Error)> AddAsync(LocalizacaoRequest request, int userId)
    {
        var route = await db.Rotas.Include(x => x.Motorista).SingleOrDefaultAsync(x => x.Id == request.RotaId);
        if (route is null) return (null, "Rota nao encontrada.");
        if (route.Motorista.UsuarioId != userId) return (null, "O motorista nao tem acesso a esta rota.");
        if (route.Status != StatusRota.Ativa) return (null, "A rota precisa estar Ativa para receber localizacoes.");
        var existing = await db.Localizacoes.SingleOrDefaultAsync(x => x.MotoristaId == route.MotoristaId && x.IdLocal == request.IdLocal);
        if (existing is not null) return (existing, "duplicate");
        var location = CreateLocation(request, route.Id, route.MotoristaId);
        db.Localizacoes.Add(location); await db.SaveChangesAsync();
        await PublishAsync(location);
        return (location, null);
    }

    public async Task<(IReadOnlyList<Localizacao> Locations, int Duplicates, string? Error)> AddBatchAsync(IEnumerable<LocalizacaoRequest> requests, int userId)
    {
        var items = requests.ToList();
        var routeIds = items.Select(x => x.RotaId).Distinct().ToList();
        var routes = await db.Rotas.Include(x => x.Motorista).Where(x => routeIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        if (routes.Count != routeIds.Count) return (Array.Empty<Localizacao>(), 0, "Uma ou mais rotas nao foram encontradas.");
        if (routes.Values.Any(x => x.Status != StatusRota.Ativa)) return (Array.Empty<Localizacao>(), 0, "Todas as rotas precisam estar Ativas.");
        if (routes.Values.Any(x => x.Motorista.UsuarioId != userId)) return (Array.Empty<Localizacao>(), 0, "O motorista nao tem acesso a uma das rotas.");
        if (items.GroupBy(x => x.IdLocal).Any(x => x.Count() > 1)) return (Array.Empty<Localizacao>(), 0, "O batch possui IdLocal duplicado.");
        var keys = items.Select(x => x.IdLocal).ToList();
        var existingKeys = await db.Localizacoes.Where(x => x.MotoristaId == userId && keys.Contains(x.IdLocal)).Select(x => x.IdLocal).ToHashSetAsync();
        var locations = items.Where(x => !existingKeys.Contains(x.IdLocal)).Select(x => CreateLocation(x, x.RotaId, routes[x.RotaId].MotoristaId)).ToList();
        db.Localizacoes.AddRange(locations); await db.SaveChangesAsync();
        foreach (var location in locations) await PublishAsync(location);
        return (locations, items.Count - locations.Count, null);
    }

    private static Localizacao CreateLocation(LocalizacaoRequest request, int routeId, int motoristaId) => new() { RotaId = routeId, MotoristaId = motoristaId, IdLocal = request.IdLocal, Latitude = request.Latitude, Longitude = request.Longitude, Velocidade = request.Velocidade, Precisao = request.Precisao, TimestampGps = request.TimestampGps.ToUniversalTime(), TimestampRecebimento = DateTime.UtcNow };
    private Task PublishAsync(Localizacao location) => hub.Clients.Group($"rota-{location.RotaId}").SendAsync("novaLocalizacao", ToDto(location));
    public static LocalizacaoDto ToDto(Localizacao x) => new(x.Id, x.RotaId, x.MotoristaId, x.Latitude, x.Longitude, x.Velocidade, x.Precisao, x.IdLocal, x.TimestampGps, x.TimestampRecebimento);
}
