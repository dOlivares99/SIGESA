using Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaquetesApiController(IPaqueteService paqueteService) : ControllerBase
{
    // GET api/paquetesapi
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var paquetes = await paqueteService.ObtenerTodosAsync();
        return Ok(paquetes);
    }

    // GET api/paquetesapi/activos  (para selectores en cotizaciones)
    [HttpGet("activos")]
    public async Task<IActionResult> GetActivos()
    {
        var paquetes = await paqueteService.ObtenerActivosAsync();
        return Ok(paquetes);
    }

    // GET api/paquetesapi/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var paquete = await paqueteService.ObtenerPorIdAsync(id);
        if (paquete == null) return NotFound();
        return Ok(paquete);
    }

    // POST api/paquetesapi
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PaqueteRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (await paqueteService.NombreExisteAsync(request.Nombre))
            return Conflict(new { mensaje = "Ya existe un paquete con ese nombre." });

        var result = await paqueteService.CrearAsync(
            request.Nombre, request.Descripcion, request.PrecioBase,
            request.MaxPersonas, request.DuracionHoras);

        return result
            ? Ok(new { mensaje = "Paquete creado correctamente." })
            : StatusCode(500, new { mensaje = "Error al crear el paquete." });
    }

    // PUT api/paquetesapi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] PaqueteRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (await paqueteService.NombreExisteAsync(request.Nombre, id))
            return Conflict(new { mensaje = "Ya existe otro paquete con ese nombre." });

        // Aviso si tiene cotizaciones en Borrador (escenario 2 edición)
        var tieneBorrador = await paqueteService.TieneCotizacionesBorradorAsync(id);

        var result = await paqueteService.ActualizarAsync(
            id, request.Nombre, request.Descripcion, request.PrecioBase,
            request.MaxPersonas, request.DuracionHoras);

        if (!result) return NotFound(new { mensaje = "Paquete no encontrado." });

        return Ok(new
        {
            mensaje = "Paquete actualizado correctamente.",
            advertencia = tieneBorrador
                ? "Este paquete tiene cotizaciones en estado Borrador. Los cambios no afectarán esas cotizaciones existentes."
                : null
        });
    }

    // PATCH api/paquetesapi/5/activo  (activar / desactivar)
    [HttpPatch("{id}/activo")]
    public async Task<IActionResult> CambiarActivo(int id, [FromBody] CambiarActivoRequest request)
    {
        var result = await paqueteService.CambiarActivoAsync(id, request.Activo);

        return result
            ? Ok(new { mensaje = request.Activo ? "Paquete activado." : "Paquete desactivado." })
            : NotFound(new { mensaje = "Paquete no encontrado." });
    }
}

public record PaqueteRequest(
    string Nombre,
    string? Descripcion,
    decimal PrecioBase,
    int MaxPersonas,
    int DuracionHoras
);

public record CambiarActivoRequest(bool Activo);