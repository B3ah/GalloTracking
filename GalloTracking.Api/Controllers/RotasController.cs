using GalloTracking.Api.Contracts;
using GalloTracking.Api.Domain;
using GalloTracking.Api.Infrastructure;
using GalloTracking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalloTracking.Api.Controllers;

[ApiController, Authorize, Route("api/rotas")]
public class RotasController(AppDbContext db, AccessService access) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] StatusRota? status, [FromQuery] int? motoristaId)
    {
        var query = db.Rotas.Include(x => x.Motorista).ThenInclude(x => x.Usuario).AsQueryable();
        if (!access.IsManager(User)) { var userId = access.UserId(User); query = query.Where(x => userId.HasValue && x.Motorista.UsuarioId == userId.Value); }
        if (status is not null) query = query.Where(x => x.Status == status); if (motoristaId is not null) query = query.Where(x => x.MotoristaId == motoristaId);
        return Ok(await query.OrderByDescending(x => x.Id).Select(x => new RotaResumoDto(x.Id, x.MotoristaId, x.Motorista.Usuario.Nome, x.Status, x.DataInicio, x.DataFim)).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        if (!await access.CanAccessRouteAsync(id, User)) return Forbid();
        var route = await db.Rotas.Include(x => x.Motorista).ThenInclude(x => x.Usuario).Include(x => x.Entregas).SingleOrDefaultAsync(x => x.Id == id);
        return route is null ? NotFound() : Ok(new RotaDetalheDto(route.Id, route.MotoristaId, route.Motorista.Usuario.Nome, route.Status, route.DataInicio, route.DataFim, route.Entregas.Select(e => new EntregaDto(e.Id, e.RotaId, e.Destinatario, e.Endereco, e.Status)).ToList()));
    }

    [HttpPost, Authorize(Roles = "Gestor")]
    public async Task<IActionResult> Create(CriarRotaRequest request)
    {
        if (!await db.Motoristas.AnyAsync(x => x.Id == request.MotoristaId)) return NotFound(new { mensagem = "Motorista nao encontrado." });
        var route = new Rota { MotoristaId = request.MotoristaId, Status = StatusRota.Planejada }; db.Rotas.Add(route); await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = route.Id }, new RotaResumoDto(route.Id, route.MotoristaId, "", route.Status, route.DataInicio, route.DataFim));
    }

    [HttpPost("{id:int}/iniciar")]
    public Task<IActionResult> Start(int id) => ChangeStatus(id, StatusRota.Ativa);

    [HttpPost("{id:int}/finalizar")]
    public Task<IActionResult> Finish(int id) => ChangeStatus(id, StatusRota.Finalizada);

    private async Task<IActionResult> ChangeStatus(int id, StatusRota status)
    {
        if (!await access.CanAccessRouteAsync(id, User)) return Forbid();
        var route = await db.Rotas.FindAsync(id); if (route is null) return NotFound();
        if ((status == StatusRota.Ativa && route.Status != StatusRota.Planejada) || (status == StatusRota.Finalizada && route.Status != StatusRota.Ativa)) return Conflict(new { mensagem = "Transicao de estado invalida." });
        route.Status = status; if (status == StatusRota.Ativa) route.DataInicio = DateTime.UtcNow; else route.DataFim = DateTime.UtcNow; await db.SaveChangesAsync(); return Ok(new { route.Id, status = route.Status, route.DataInicio, route.DataFim });
    }
}
