namespace WEB.Models.ViewModels;

public class UsuarioSesionViewModel
{
    public int    UsuarioId  { get; set; }
    public string Nombre     { get; set; } = string.Empty;
    public string Email      { get; set; } = string.Empty;
    public string RolNombre  { get; set; } = string.Empty;
}
