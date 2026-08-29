using GalloTracking.Api.Contracts;
using GalloTracking.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GalloTracking.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request) => (await auth.LoginAsync(request)) is { } response ? Ok(response) : Unauthorized(new { mensagem = "Credenciais inválidas." });

    [HttpPost("logout"), Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Logout() => NoContent();
}
