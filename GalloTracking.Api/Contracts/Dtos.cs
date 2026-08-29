using System.ComponentModel.DataAnnotations;
using GalloTracking.Api.Domain;

namespace GalloTracking.Api.Contracts;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Senha { get; set; } = string.Empty;
}
public record LoginResponse(string Token, int UsuarioId, string Nome, string Perfil);
public class CriarRotaRequest
{
    [Range(1, int.MaxValue)]
    public int MotoristaId { get; set; }
}
public class CriarEntregaRequest
{
    [Range(1, int.MaxValue)]
    public int RotaId { get; set; }

    [Required, StringLength(160)]
    public string Destinatario { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Endereco { get; set; } = string.Empty;
}
public class AtualizarEntregaRequest
{
    public StatusEntrega Status { get; set; }
}
public class LocalizacaoRequest
{
    [Range(1, int.MaxValue)]
    public int RotaId { get; set; }

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Range(0, double.MaxValue)]
    public double Velocidade { get; set; }

    [Range(0, double.MaxValue)]
    public double Precisao { get; set; }

    [Required, StringLength(100)]
    public string IdLocal { get; set; } = string.Empty;

    public DateTime TimestampGps { get; set; }
}
public record LocalizacaoDto(int Id, int RotaId, int MotoristaId, double Latitude, double Longitude, double Velocidade, double Precisao, string IdLocal, DateTime TimestampGps, DateTime TimestampRecebimento);
public record EntregaDto(int Id, int RotaId, string Destinatario, string Endereco, StatusEntrega Status);
public record RotaResumoDto(int Id, int MotoristaId, string Motorista, StatusRota Status, DateTime? DataInicio, DateTime? DataFim);
public record RotaDetalheDto(int Id, int MotoristaId, string Motorista, StatusRota Status, DateTime? DataInicio, DateTime? DataFim, IReadOnlyList<EntregaDto> Entregas);
