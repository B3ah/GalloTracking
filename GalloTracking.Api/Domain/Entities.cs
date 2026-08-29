namespace GalloTracking.Api.Domain;

public enum StatusRota { Planejada, Ativa, Finalizada }
public enum StatusEntrega { Pendente, EmTransito, Entregue, Cancelada }
public enum PerfilUsuario { Gestor, Motorista }

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public PerfilUsuario Perfil { get; set; }
    public Motorista? Motorista { get; set; }
}

public class Motorista
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string Telefone { get; set; } = "";
    public ICollection<Rota> Rotas { get; set; } = new List<Rota>();
}

public class Rota
{
    public int Id { get; set; }
    public int MotoristaId { get; set; }
    public Motorista Motorista { get; set; } = null!;
    public StatusRota Status { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();
    public ICollection<Localizacao> Localizacoes { get; set; } = new List<Localizacao>();
}

public class Entrega
{
    public int Id { get; set; }
    public int RotaId { get; set; }
    public Rota Rota { get; set; } = null!;
    public string Destinatario { get; set; } = "";
    public string Endereco { get; set; } = "";
    public StatusEntrega Status { get; set; }
}

public class Localizacao
{
    public int Id { get; set; }
    public int RotaId { get; set; }
    public Rota Rota { get; set; } = null!;
    public int MotoristaId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Velocidade { get; set; }
    public double Precisao { get; set; }
    public DateTime TimestampGps { get; set; }
    public DateTime TimestampRecebimento { get; set; }
}
