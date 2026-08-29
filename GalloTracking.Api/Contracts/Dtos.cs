using GalloTracking.Api.Domain;

namespace GalloTracking.Api.Contracts;

public record LoginRequest(string Email, string Senha);
public record LoginResponse(string Token, int UsuarioId, string Nome, string Perfil);
public record CriarRotaRequest(int MotoristaId);
public record CriarEntregaRequest(int RotaId, string Destinatario, string Endereco);
public record AtualizarEntregaRequest(StatusEntrega Status);
public record LocalizacaoRequest(int RotaId, double Latitude, double Longitude, double Velocidade, double Precisao, DateTime TimestampGps);
public record LocalizacaoDto(int Id, int RotaId, int MotoristaId, double Latitude, double Longitude, double Velocidade, double Precisao, DateTime TimestampGps, DateTime TimestampRecebimento);
