using API.Services;
using Business.Services;
using Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecuperacionApiController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IRecuperacionRepository _recuperacionRepo;
    private readonly IEmailService _emailService;

    public RecuperacionApiController(
        IUsuarioRepository usuarioRepo,
        IRecuperacionRepository recuperacionRepo,
        IEmailService emailService)
    {
        _usuarioRepo = usuarioRepo;
        _recuperacionRepo = recuperacionRepo;
        _emailService = emailService;
    }

    // POST api/recuperacionapi/solicitar
    [HttpPost("solicitar")]
    public async Task<IActionResult> Solicitar([FromBody] SolicitarRequest request)
    {
        var usuario = await _usuarioRepo.FindByEmailAsync(request.Email);
        if (usuario == null || !usuario.Activo)
            return Ok(new { mensaje = "Si el correo existe, recibirás un enlace en breve." });

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var expiracion = DateTime.UtcNow.AddHours(2);

        await _recuperacionRepo.GuardarTokenAsync(usuario.UsuarioId, token, expiracion);

        try
        {
            await _emailService.EnviarRecuperacionAsync(usuario.Email, usuario.Nombre, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email Error] {ex.Message}");
            return StatusCode(500, new { mensaje = "No se pudo enviar el correo. Revisa la configuración SMTP." });
        }

        return Ok(new { mensaje = "Si el correo existe, recibirás un enlace en breve." });
    }

    // POST api/recuperacionapi/restablecer
    [HttpPost("restablecer")]
    public async Task<IActionResult> Restablecer([FromBody] RestablecerRequest request)
    {
        var registro = await _recuperacionRepo.BuscarTokenValidoAsync(request.Token);
        if (registro == null)
            return BadRequest(new { mensaje = "El enlace no es válido o ya expiró." });

        var usuario = registro.Usuario;
        if (usuario == null)
            return BadRequest(new { mensaje = "Usuario no encontrado." });

        usuario.PasswordHash = UsuarioService.HashPassword(request.NuevaContrasena);
        await _usuarioRepo.UpdateAsync(usuario);

        await _recuperacionRepo.InvalidarTokenAsync(request.Token);
        await _recuperacionRepo.LimpiarExpiradosAsync();

        return Ok(new { mensaje = "Contraseña actualizada correctamente." });
    }

    // GET api/recuperacionapi/validar?token=...
    [HttpGet("validar")]
    public async Task<IActionResult> Validar([FromQuery] string token)
    {
        var registro = await _recuperacionRepo.BuscarTokenValidoAsync(token);
        return registro != null
            ? Ok(new { valido = true })
            : Ok(new { valido = false });
    }
}

public record SolicitarRequest(string Email);
public record RestablecerRequest(string Token, string NuevaContrasena);