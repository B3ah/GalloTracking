using GalloTracking.Api.Contracts;
using GalloTracking.Api.Domain;
using GalloTracking.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Controllers;

[ApiController, Authorize, Route("api")]
public class EntregasController(AppDbContext db) : ControllerBase
{
    [HttpPost("entregas")]
    public async Task<IActionResult> Create(CriarEntregaRequest request)
    { if (!await db.Rotas.AnyAsync(x => x.Id == request.RotaId)) return NotFound(); var e = new Entrega { RotaId = request.RotaId, Destinatario = request.Destinatario, Endereco = request.Endereco, Status = StatusEntrega.Pendente }; db.Entregas.Add(e); await db.SaveChangesAsync(); return Created($"/api/entregas/{e.Id}", e); }

    [HttpGet("rotas/{rotaId:int}/entregas")]
    public async Task<IActionResult> List(int rotaId) => Ok(await db.Entregas.Where(x => x.RotaId == rotaId).ToListAsync());

    [HttpPatch("entregas/{id:int}/status")]
    public async Task<IActionResult> Status(int id, AtualizarEntregaRequest request) { var e = await db.Entregas.FindAsync(id); if (e is null) return NotFound(); e.Status = request.Status; await db.SaveChangesAsync(); return Ok(e); }
}
