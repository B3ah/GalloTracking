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
    public async Task Login_invalido_retornar_401()
    {
        using var factory = new ApiFactory(); using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "nao-existe@gallo.local", senha = "errada" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_protegido_sem_token_retornar_401()
    {
        using var factory = new ApiFactory(); using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/rotas")).StatusCode);
    }

    [Fact]
    public async Task Ciclo_da_rota_rejeita_transicoes_invalidas()
    {
        using var factory = new ApiFactory(); using var client = await AuthenticatedClient(factory, "motorista@gallo.local");
        var route = (await client.GetFromJsonAsync<List<RouteResult>>("/api/rotas"))!.Single();
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync($"/api/rotas/{route.Id}/finalizar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/rotas/{route.Id}/iniciar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync($"/api/rotas/{route.Id}/iniciar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/rotas/{route.Id}/finalizar", null)).StatusCode);
    }

    [Fact]
    public async Task Criar_rota_e_filtrar_por_status()
    {
        using var factory = new ApiFactory(); using var client = await AuthenticatedClient(factory, "gestor@gallo.local");
        var created = await client.PostAsJsonAsync("/api/rotas", new { motoristaId = 1 });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var planned = await client.GetFromJsonAsync<List<RouteResult>>("/api/rotas?status=Planejada");
        Assert.Contains(planned!, route => route.Status == "Planejada");
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/rotas/99999")).StatusCode);
    }

    [Fact]
    public async Task Entrega_pode_ser_criada_consultada_e_atualizada()
    {
        using var factory = new ApiFactory(); using var client = await AuthenticatedClient(factory, "gestor@gallo.local");
        var created = await client.PostAsJsonAsync("/api/entregas", new { rotaId = 1, destinatario = "Cliente Teste", endereco = "Rua A, 10" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var deliveries = await client.GetFromJsonAsync<List<DeliveryResult>>("/api/rotas/1/entregas");
        var delivery = deliveries!.Single(x => x.Destinatario == "Cliente Teste");
        var updated = await client.PatchAsJsonAsync($"/api/entregas/{delivery.Id}/status", new { status = "Entregue" });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/entregas", new { rotaId = 999, destinatario = "x", endereco = "y" })).StatusCode);
    }

    [Fact]
    public async Task Localizacao_individual_historico_ultima_e_batch_funcionam()
    {
        using var factory = new ApiFactory(); using var client = await AuthenticatedClient(factory, "motorista@gallo.local");
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/api/rotas/1/iniciar", null)).StatusCode);
        var first = new { rotaId = 1, latitude = -22.5, longitude = -48.5, velocidade = 40, precisao = 5, timestampGps = DateTime.UtcNow.AddMinutes(-2) };
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/localizacoes", first)).StatusCode);
        var batch = new[] { new { rotaId = 1, latitude = -22.51, longitude = -48.51, velocidade = 41, precisao = 5, timestampGps = DateTime.UtcNow } };
        var batchResponse = await client.PostAsJsonAsync("/api/localizacoes/batch", batch);
        Assert.Equal(HttpStatusCode.OK, batchResponse.StatusCode);
        var history = await client.GetFromJsonAsync<List<LocationResult>>("/api/rotas/1/localizacoes");
        Assert.Equal(2, history!.Count);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/rotas/1/ultima-localizacao")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/localizacoes/batch", new[] { new { rotaId = 999, latitude = 0d, longitude = 0d, velocidade = 0d, precisao = 0d, timestampGps = DateTime.UtcNow } })).StatusCode);
    }

    [Fact]
    public async Task Localizacao_em_rota_finalizada_e_rejeitada()
    {
        using var factory = new ApiFactory(); using var client = await AuthenticatedClient(factory, "gestor@gallo.local");
        await client.PostAsync("/api/rotas/1/iniciar", null); await client.PostAsync("/api/rotas/1/finalizar", null);
        var response = await client.PostAsJsonAsync("/api/localizacoes", new { rotaId = 1, latitude = 0d, longitude = 0d, velocidade = 0d, precisao = 1d, timestampGps = DateTime.UtcNow });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/rotas/1/ultima-localizacao")).StatusCode);
    }

    private static async Task<HttpClient> AuthenticatedClient(ApiFactory factory, string email)
    {
        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, senha = "gallo123" });
        login.EnsureSuccessStatusCode();
        var result = (await login.Content.ReadFromJsonAsync<LoginResult>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
        return client;
    }

    private sealed record LoginResult(string Token);
    private sealed record RouteResult(int Id, string? Status);
    private sealed record DeliveryResult(int Id, string Destinatario);
    private sealed record LocationResult(int Id);
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string database = Path.Combine(Path.GetTempPath(), $"gallo-tests-{Guid.NewGuid():N}.db");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={database}");
        builder.ConfigureLogging(logging => logging.ClearProviders());
    }
}
