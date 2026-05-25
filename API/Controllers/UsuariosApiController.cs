using Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuariosApiController(IUsuarioService usuarioService) : ControllerBase
{
    // GET api/usuariosapi
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var usuarios = await usuarioService.ObtenerTodosAsync();
        return Ok(usuarios);
    }

    // GET api/usuariosapi/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var usuario = await usuarioService.ObtenerPorIdAsync(id);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    // POST api/usuariosapi/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var usuario = await usuarioService.AutenticarAsync(request.Email, request.Password);
        if (usuario == null)
            return Unauthorized(new { mensaje = "Credenciales incorrectas." });

        return Ok(usuario);
    }

    // POST api/usuariosapi
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CrearUsuarioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var emailExiste = await usuarioService.EmailExisteAsync(request.Email);
        if (emailExiste)
            return Conflict(new { mensaje = "El correo ya está registrado." });

        var result = await usuarioService.CrearAsync(
            request.Nombre, request.Email, request.Password, request.RolId);

        return result
            ? Ok(new { mensaje = "Usuario creado correctamente." })
            : StatusCode(500, new { mensaje = "Error al crear el usuario." });
    }

    // PUT api/usuariosapi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] ActualizarUsuarioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await usuarioService.ActualizarAsync(
            id, request.Nombre, request.Email, request.RolId);

        return result
            ? Ok(new { mensaje = "Usuario actualizado correctamente." })
            : NotFound(new { mensaje = "Usuario no encontrado." });
    }
}

public record LoginRequest(string Email, string Password);
public record CrearUsuarioRequest(string Nombre, string Email, string Password, int RolId);
public record ActualizarUsuarioRequest(string Nombre, string Email, int RolId);
