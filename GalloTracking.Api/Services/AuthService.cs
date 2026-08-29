using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GalloTracking.Api.Contracts;
using GalloTracking.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GalloTracking.Api.Services;

public class AuthService(AppDbContext db, IConfiguration config)
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await db.Usuarios.SingleOrDefaultAsync(x => x.Email == request.Email);
        if (user is null || new PasswordHasher<Domain.Usuario>().VerifyHashedPassword(user, user.SenhaHash, request.Senha) == PasswordVerificationResult.Failed) return null;
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Nome), new Claim(ClaimTypes.Role, user.Perfil.ToString()) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), user.Id, user.Nome, user.Perfil.ToString());
    }
}
