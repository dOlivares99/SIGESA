using Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiciosApiController(IServicioService servicioService) : ControllerBase
{
    // GET api/serviciosapi
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var servicios = await servicioService.ObtenerTodosAsync();
        return Ok(servicios);
    }

    // GET api/serviciosapi/activos
    [HttpGet("activos")]
    public async Task<IActionResult> GetActivos()
    {
        var servicios = await servicioService.ObtenerActivosAsync();
        return Ok(servicios);
    }

    // GET api/serviciosapi/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var servicio = await servicioService.ObtenerPorIdAsync(id);
        if (servicio == null) return NotFound();
        return Ok(servicio);
    }

    // POST api/serviciosapi
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ServicioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Escenario 2: nombre duplicado
        if (await servicioService.NombreExisteAsync(request.Nombre))
            return Conflict(new { mensaje = "Ya existe un servicio con ese nombre." });

        var result = await servicioService.CrearAsync(
            request.Nombre, request.Categoria, request.PrecioUnitario);

        return result
            ? Ok(new { mensaje = "Servicio registrado correctamente." })
            : StatusCode(500, new { mensaje = "Error al registrar el servicio." });
    }

    // PUT api/serviciosapi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] ServicioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Escenario 2 edición: nombre duplicado en otro registro
        if (await servicioService.NombreExisteAsync(request.Nombre, id))
            return Conflict(new { mensaje = "Ya existe otro servicio con ese nombre." });

        var result = await servicioService.ActualizarAsync(
            id, request.Nombre, request.Categoria, request.PrecioUnitario);

        return result
            ? Ok(new { mensaje = "Servicio actualizado correctamente." })
            : NotFound(new { mensaje = "Servicio no encontrado." });
    }

    // PATCH api/serviciosapi/5/activo
    [HttpPatch("{id}/activo")]
    public async Task<IActionResult> CambiarActivo(int id,
        [FromBody] CambiarActivoRequest request)
    {
        var result = await servicioService.CambiarActivoAsync(id, request.Activo);

        return result
            ? Ok(new { mensaje = request.Activo ? "Servicio activado." : "Servicio desactivado." })
            : NotFound(new { mensaje = "Servicio no encontrado." });
    }
}

public record ServicioRequest(string Nombre, string Categoria, decimal PrecioUnitario);
public record CambiarActivoServicioRequest(bool Activo);