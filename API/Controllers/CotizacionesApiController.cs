using Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CotizacionesApiController(ICotizacionService cotizacionService) : ControllerBase
{
    // GET api/cotizacionesapi
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var cotizaciones = await cotizacionService.ObtenerTodasAsync();
        return Ok(cotizaciones);
    }

    // GET api/cotizacionesapi/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var cotizacion = await cotizacionService.ObtenerPorIdAsync(id);
        if (cotizacion == null) return NotFound();
        return Ok(cotizacion);
    }

    // POST api/cotizacionesapi
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CrearCotizacionRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await cotizacionService.CrearAsync(
            request.EventoId,
            request.FechaVencimiento,
            request.UsuarioId);

        return result
            ? Ok(new { mensaje = "Cotización creada correctamente." })
            : StatusCode(500, new { mensaje = "Error al crear la cotización." });
    }

    // PUT api/cotizacionesapi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id,
        [FromBody] ActualizarCotizacionRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await cotizacionService.ActualizarEstadoAsync(
            id,
            request.Estado,
            request.MotivoRechazo,
            request.Total,
            request.FechaVencimiento);

        return result
            ? Ok(new { mensaje = "Cotización actualizada correctamente." })
            : NotFound(new { mensaje = "Cotización no encontrada." });
    }
}

public record CrearCotizacionRequest(
    int EventoId,
    DateTime? FechaVencimiento,
    int UsuarioId
);

public record ActualizarCotizacionRequest(
    string Estado,
    string? MotivoRechazo,
    decimal Total,
    DateTime? FechaVencimiento
);