using GalloTracking.Api.Contracts;
using GalloTracking.Api.Domain;
using GalloTracking.Api.Infrastructure;
using GalloTracking.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Services;

public class TrackingService(AppDbContext db, IHubContext<LocalizacaoHub> hub)
{
    public async Task<(Localizacao? Location, string? Error)> AddAsync(LocalizacaoRequest request)
    {
        var route = await db.Rotas.FindAsync(request.RotaId);
        if (route is null) return (null, "Rota não encontrada.");
        if (route.Status != StatusRota.Ativa) return (null, "A rota precisa estar Ativa para receber localizações.");
        var location = new Localizacao { RotaId = route.Id, MotoristaId = route.MotoristaId, Latitude = request.Latitude, Longitude = request.Longitude, Velocidade = request.Velocidade, Precisao = request.Precisao, TimestampGps = request.TimestampGps, TimestampRecebimento = DateTime.UtcNow };
        db.Localizacoes.Add(location); await db.SaveChangesAsync();
        await hub.Clients.Group($"rota-{route.Id}").SendAsync("novaLocalizacao", ToDto(location));
        return (location, null);
    }

    public async Task<(IReadOnlyList<Localizacao> Locations, string? Error)> AddBatchAsync(IEnumerable<LocalizacaoRequest> requests)
    {
        var items = requests.ToList();
        var routeIds = items.Select(x => x.RotaId).Distinct().ToList();
        var routes = await db.Rotas.Where(x => routeIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        if (routes.Count != routeIds.Count) return (Array.Empty<Localizacao>(), "Uma ou mais rotas não foram encontradas.");
        if (routes.Values.Any(x => x.Status != StatusRota.Ativa)) return (Array.Empty<Localizacao>(), "Todas as rotas precisam estar Ativas para receber localizações.");
        var locations = items.Select(x => new Localizacao { RotaId = x.RotaId, MotoristaId = routes[x.RotaId].MotoristaId, Latitude = x.Latitude, Longitude = x.Longitude, Velocidade = x.Velocidade, Precisao = x.Precisao, TimestampGps = x.TimestampGps, TimestampRecebimento = DateTime.UtcNow }).ToList();
        db.Localizacoes.AddRange(locations); await db.SaveChangesAsync();
        foreach (var location in locations) await hub.Clients.Group($"rota-{location.RotaId}").SendAsync("novaLocalizacao", ToDto(location));
        return (locations, null);
    }

    public static LocalizacaoDto ToDto(Localizacao x) => new(x.Id, x.RotaId, x.MotoristaId, x.Latitude, x.Longitude, x.Velocidade, x.Precisao, x.TimestampGps, x.TimestampRecebimento);
}
