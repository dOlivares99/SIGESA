using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels;

public class RecuperarContrasenaViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    public string Email { get; set; } = string.Empty;
}

public class NuevaContrasenaViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "Mínimo 8 caracteres.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña.")]
    [DataType(DataType.Password)]
    [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}