namespace WEB.Models.ViewModels;

public class UsuarioViewModel
{
    public int          UsuarioId { get; set; }
    public string       Nombre    { get; set; } = string.Empty;
    public string       Email     { get; set; } = string.Empty;
    public int          RolId     { get; set; }
    public bool         Activo    { get; set; }
    public RolViewModel? Rol      { get; set; }
    public string       RolNombre { get; set; } = string.Empty;
}

public class RolViewModel
{
    public int    RolId  { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
