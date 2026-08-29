using GalloTracking.Api.Contracts;
using GalloTracking.Api.Infrastructure;
using GalloTracking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Controllers;

[ApiController, Authorize, Route("api")]
public class LocalizacoesController(AppDbContext db, TrackingService tracking, AccessService access) : ControllerBase
{
    [HttpPost("localizacoes"), Authorize(Roles = "Motorista")]
    public async Task<IActionResult> Add(LocalizacaoRequest request)
    {
        var userId = access.UserId(User); if (!userId.HasValue) return Forbid();
        var result = await tracking.AddAsync(request, userId.Value);
        if (result.Error == "duplicate") return Ok(new { duplicada = true, localizacao = TrackingService.ToDto(result.Location!) });
        return result.Location is { } location ? Ok(TrackingService.ToDto(location)) : Conflict(new { mensagem = result.Error });
    }

    [HttpPost("localizacoes/batch"), Authorize(Roles = "Motorista")]
    public async Task<IActionResult> Batch(IEnumerable<LocalizacaoRequest> requests)
    {
        var items = requests.ToList(); if (items.Count == 0 || items.Count > 500) return Problem("O batch deve conter entre 1 e 500 localizacoes.", statusCode: 422);
        var userId = access.UserId(User); if (!userId.HasValue) return Forbid();
        var result = await tracking.AddBatchAsync(items, userId.Value); if (result.Error is not null) return Conflict(new { mensagem = result.Error });
        return Ok(new { processadas = result.Locations.Count, duplicadas = result.Duplicates, localizacoes = result.Locations.Select(TrackingService.ToDto) });
    }

    [HttpGet("rotas/{rotaId:int}/localizacoes")]
    public async Task<IActionResult> History(int rotaId)
    {
        if (!await access.CanAccessRouteAsync(rotaId, User)) return Forbid();
        return Ok((await db.Localizacoes.Where(x => x.RotaId == rotaId).OrderBy(x => x.TimestampGps).ToListAsync()).Select(TrackingService.ToDto));
    }

    [HttpGet("rotas/{rotaId:int}/ultima-localizacao")]
    public async Task<IActionResult> Last(int rotaId)
    {
        if (!await access.CanAccessRouteAsync(rotaId, User)) return Forbid();
        return await db.Localizacoes.Where(x => x.RotaId == rotaId).OrderByDescending(x => x.TimestampGps).FirstOrDefaultAsync() is { } x ? Ok(TrackingService.ToDto(x)) : NotFound();
    }
}
