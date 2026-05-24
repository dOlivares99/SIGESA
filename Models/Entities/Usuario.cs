namespace Data;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Auditoria> Auditoria { get; set; } = [];

    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = [];

    public virtual ICollection<Evento> Eventos { get; set; } = [];

    public virtual ICollection<Pago> Pagos { get; set; } = [];

    public virtual Rol Rol { get; set; } = null!;
}
