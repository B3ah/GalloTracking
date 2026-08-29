using GalloTracking.Api.Contracts;
using GalloTracking.Api.Infrastructure;
using GalloTracking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Controllers;

[ApiController, Authorize, Route("api")]
public class LocalizacoesController(AppDbContext db, TrackingService tracking) : ControllerBase
{
    [HttpPost("localizacoes")]
    public async Task<IActionResult> Add(LocalizacaoRequest request) => await AddOne(request);

    [HttpPost("localizacoes/batch")]
    public async Task<IActionResult> Batch(IEnumerable<LocalizacaoRequest> requests)
    { var added = await tracking.AddBatchAsync(requests); if (added.Error is not null) return Conflict(new { mensagem = added.Error }); return Ok(new { processadas = added.Locations.Count, localizacoes = added.Locations.Select(TrackingService.ToDto) }); }

    [HttpGet("rotas/{rotaId:int}/localizacoes")]
    public async Task<IActionResult> History(int rotaId) => Ok((await db.Localizacoes.Where(x => x.RotaId == rotaId).OrderBy(x => x.TimestampGps).ToListAsync()).Select(TrackingService.ToDto));

    [HttpGet("rotas/{rotaId:int}/ultima-localizacao")]
    public async Task<IActionResult> Last(int rotaId) => await db.Localizacoes.Where(x => x.RotaId == rotaId).OrderByDescending(x => x.TimestampGps).FirstOrDefaultAsync() is { } x ? Ok(TrackingService.ToDto(x)) : NotFound();

    private async Task<IActionResult> AddOne(LocalizacaoRequest request) { var result = await tracking.AddAsync(request); return result.Location is { } x ? Ok(TrackingService.ToDto(x)) : Conflict(new { mensagem = result.Error }); }
}
