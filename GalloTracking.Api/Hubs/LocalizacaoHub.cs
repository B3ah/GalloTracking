using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using GalloTracking.Api.Services;

namespace GalloTracking.Api.Hubs;

[Authorize]
public class LocalizacaoHub(AccessService access) : Hub
{
    public async Task EntrarNaRota(int rotaId)
    {
        if (!await access.CanAccessRouteAsync(rotaId, Context.User!)) throw new HubException("Acesso negado para esta rota.");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"rota-{rotaId}");
    }
    public Task SairDaRota(int rotaId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"rota-{rotaId}");
}
