using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace GalloTracking.Api.Tests;

public class ApiTests
{
    [Fact]
    public async Task Login_e_ciclo_da_rota_funcionam()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "motorista@gallo.local", senha = "gallo123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = (await login.Content.ReadFromJsonAsync<LoginResult>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var routes = await client.GetFromJsonAsync<List<RouteResult>>("/api/rotas");
        var routeId = routes!.Single().Id;
        var start = await client.PostAsync($"/api/rotas/{routeId}/iniciar", null);
        Assert.Equal(HttpStatusCode.OK, start.StatusCode);
        var finish = await client.PostAsync($"/api/rotas/{routeId}/finalizar", null);
        Assert.Equal(HttpStatusCode.OK, finish.StatusCode);
    }

    [Fact]
    public async Task Localizacao_fora_de_rota_ativa_e_rejeitada()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "gestor@gallo.local", senha = "gallo123" });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await login.Content.ReadFromJsonAsync<LoginResult>())!.Token);
        var response = await client.PostAsJsonAsync("/api/localizacoes", new { rotaId = 1, latitude = -22.5, longitude = -48.5, velocidade = 0, precisao = 5, timestampGps = DateTime.UtcNow });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private sealed record LoginResult(string Token);
    private sealed record RouteResult(int Id);
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string database = Path.Combine(Path.GetTempPath(), $"gallo-tests-{Guid.NewGuid():N}.db");
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={database}");
        builder.ConfigureLogging(logging => logging.ClearProviders());
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
