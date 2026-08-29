using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GalloTracking.Api.Hubs;

[Authorize]
public class LocalizacaoHub : Hub
{
    public Task EntrarNaRota(int rotaId) => Groups.AddToGroupAsync(Context.ConnectionId, $"rota-{rotaId}");
    public Task SairDaRota(int rotaId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"rota-{rotaId}");
}
