using Data.Repositories;
using Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservasApiController(IReservaService reservaService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var reservas = await reservaService.ObtenerTodosAsync();
        return Ok(reservas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var reserva = await reservaService.ObtenerConDetallesAsync(id);
        if (reserva == null) return NotFound();
        return Ok(reserva);
    }
    // GET api/reservasapi/calendario
[HttpGet("calendario")]
public async Task<IActionResult> GetCalendario()
{
    var reservas = await reservaService.ObtenerTodosAsync();
    var eventos = reservas.Select(r => new
    {
        id = r.EventoId,
        title = r.TipoEvento,
        start = r.FechaEvento.ToString("yyyy-MM-dd"),
        color = r.Estado switch
        {
            "Confirmada" => "#534AB7",
            "EnProceso"  => "#854F0B",
            "Realizada"  => "#0F6E56",
            "Cancelada"  => "#993C1D",
            _            => "#6c757d"
        },
        extendedProps = new
        {
            estado = r.Estado,
            eventoId = r.EventoId
        }
    });
    return Ok(eventos);
}

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CrearReservaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var dto = new CrearReservaDto(
            request.ClienteId,
            request.PaqueteId,
            request.TipoEvento,
            DateOnly.FromDateTime(request.FechaEvento),
            request.NumPersonas,
            request.MontoTotal,
            request.Notas,
            request.Servicios.Select(s =>
                new EventoServicioDto(s.ServicioId, s.Cantidad, s.PrecioAcordado)).ToList()
        );

        var result = await reservaService.CrearAsync(dto, request.UsuarioCreacion);

        return result
            ? Ok(new { mensaje = "Reserva creada correctamente." })
            : StatusCode(500, new { mensaje = "Error al crear la reserva." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] ActualizarReservaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var dto = new ActualizarReservaDto(
            request.ClienteId,
            request.PaqueteId,
            request.TipoEvento,
            DateOnly.FromDateTime(request.FechaEvento),
            request.NumPersonas,
            request.Estado,
            request.MontoTotal,
            request.Notas,
            request.Servicios.Select(s =>
                new EventoServicioDto(s.ServicioId, s.Cantidad, s.PrecioAcordado)).ToList()
        );

        var result = await reservaService.ActualizarAsync(id, dto);

        return result
            ? Ok(new { mensaje = "Reserva actualizada correctamente." })
            : NotFound(new { mensaje = "Reserva no encontrada." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await reservaService.EliminarAsync(id);

        return result
            ? Ok(new { mensaje = "Reserva eliminada correctamente." })
            : NotFound(new { mensaje = "Reserva no encontrada." });
    }
}

public record ServicioItemRequest(int ServicioId, int Cantidad, decimal PrecioAcordado);

public record CrearReservaRequest(
    int ClienteId,
    int PaqueteId,
    string TipoEvento,
    DateTime FechaEvento,
    int NumPersonas,
    decimal MontoTotal,
    string? Notas,
    int UsuarioCreacion,
    List<ServicioItemRequest> Servicios
);

public record ActualizarReservaRequest(
    int ClienteId,
    int PaqueteId,
    string TipoEvento,
    DateTime FechaEvento,
    int NumPersonas,
    string Estado,
    decimal MontoTotal,
    string? Notas,
    List<ServicioItemRequest> Servicios
);