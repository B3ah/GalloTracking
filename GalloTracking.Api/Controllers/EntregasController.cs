using GalloTracking.Api.Contracts;
using GalloTracking.Api.Domain;
using GalloTracking.Api.Infrastructure;
using GalloTracking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Controllers;

[ApiController, Authorize, Route("api")]
public class EntregasController(AppDbContext db, AccessService access) : ControllerBase
{
    [HttpPost("entregas"), Authorize(Roles = "Gestor")]
    public async Task<IActionResult> Create(CriarEntregaRequest request)
    {
        if (!await access.CanAccessRouteAsync(request.RotaId, User)) return Forbid();
        if (!await db.Rotas.AnyAsync(x => x.Id == request.RotaId)) return NotFound(new { mensagem = "Rota nao encontrada." });
        var delivery = new Entrega { RotaId = request.RotaId, Destinatario = request.Destinatario.Trim(), Endereco = request.Endereco.Trim(), Status = StatusEntrega.Pendente }; db.Entregas.Add(delivery); await db.SaveChangesAsync();
        return Created($"/api/entregas/{delivery.Id}", ToDto(delivery));
    }

    [HttpGet("rotas/{rotaId:int}/entregas")]
    public async Task<IActionResult> List(int rotaId)
    { if (!await access.CanAccessRouteAsync(rotaId, User)) return Forbid(); return Ok((await db.Entregas.Where(x => x.RotaId == rotaId).OrderBy(x => x.Id).ToListAsync()).Select(ToDto)); }

    [HttpPatch("entregas/{id:int}/status"), Authorize(Roles = "Gestor")]
    public async Task<IActionResult> Status(int id, AtualizarEntregaRequest request)
    { var delivery = await db.Entregas.FindAsync(id); if (delivery is null) return NotFound(); if (!await access.CanAccessRouteAsync(delivery.RotaId, User)) return Forbid(); delivery.Status = request.Status; await db.SaveChangesAsync(); return Ok(ToDto(delivery)); }

    private static EntregaDto ToDto(Entrega x) => new(x.Id, x.RotaId, x.Destinatario, x.Endereco, x.Status);
}
