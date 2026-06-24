namespace Models.Entities;

public class TokenRecuperacion
{
    public int TokenRecuperacionId { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public bool Usado { get; set; }
    public virtual Usuario? Usuario { get; set; }
}
