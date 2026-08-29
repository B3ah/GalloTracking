using GalloTracking.Api.Contracts;
using GalloTracking.Api.Domain;
using GalloTracking.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Controllers;

[ApiController, Authorize, Route("api/rotas")]
public class RotasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] StatusRota? status, [FromQuery] int? motoristaId) => Ok(await db.Rotas.Include(x => x.Motorista).ThenInclude(x => x.Usuario).Where(x => status == null || x.Status == status).Where(x => motoristaId == null || x.MotoristaId == motoristaId).Select(x => new { x.Id, x.MotoristaId, motorista = x.Motorista.Usuario.Nome, status = x.Status.ToString(), x.DataInicio, x.DataFim }).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) => await db.Rotas.Include(x => x.Motorista).ThenInclude(x => x.Usuario).Include(x => x.Entregas).SingleOrDefaultAsync(x => x.Id == id) is { } r ? Ok(r) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(CriarRotaRequest request)
    {
        if (!await db.Motoristas.AnyAsync(x => x.Id == request.MotoristaId)) return NotFound(new { mensagem = "Motorista não encontrado." });
        var route = new Rota { MotoristaId = request.MotoristaId, Status = StatusRota.Planejada }; db.Rotas.Add(route); await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = route.Id }, route);
    }

    [HttpPost("{id:int}/iniciar")]
    public Task<IActionResult> Start(int id) => ChangeStatus(id, StatusRota.Ativa);

    [HttpPost("{id:int}/finalizar")]
    public Task<IActionResult> Finish(int id) => ChangeStatus(id, StatusRota.Finalizada);

    private async Task<IActionResult> ChangeStatus(int id, StatusRota status)
    {
        var r = await db.Rotas.FindAsync(id); if (r is null) return NotFound();
        if ((status == StatusRota.Ativa && r.Status != StatusRota.Planejada) || (status == StatusRota.Finalizada && r.Status != StatusRota.Ativa)) return Conflict(new { mensagem = "Transição de estado inválida." });
        r.Status = status; if (status == StatusRota.Ativa) r.DataInicio = DateTime.UtcNow; else r.DataFim = DateTime.UtcNow; await db.SaveChangesAsync(); return Ok(r);
    }
}
