using API.Services;
using Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PagosApiController(IPagoService pagoService, IBlobStorageService blobService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var pagos = await pagoService.ObtenerTodosAsync();
        return Ok(pagos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var pago = await pagoService.ObtenerPorIdAsync(id);
        if (pago == null) return NotFound();
        return Ok(pago);
    }

    [HttpGet("evento/{eventoId}")]
    public async Task<IActionResult> GetPorEvento(int eventoId)
    {
        var pagos = await pagoService.ObtenerPorEventoAsync(eventoId);
        return Ok(pagos);
    }

    [HttpGet("tipos")]
    public async Task<IActionResult> GetTipos()
    {
        var tipos = await pagoService.ObtenerTiposPagoAsync();
        return Ok(tipos);
    }

    [HttpGet("metodos")]
    public async Task<IActionResult> GetMetodos()
    {
        var metodos = await pagoService.ObtenerMetodosPagoAsync();
        return Ok(metodos);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Post([FromForm] CrearPagoForm form)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        string? urlComprobante = null;

        // Escenarios 2 y 3: subir comprobante si viene, validar formato
        if (form.Comprobante is { Length: > 0 })
        {
            await using var stream = form.Comprobante.OpenReadStream();
            urlComprobante = await blobService.SubirComprobanteAsync(
                stream, form.Comprobante.FileName, form.Comprobante.ContentType);

            if (urlComprobante == null)
                return BadRequest(new { mensaje = "Solo se permiten imágenes JPG o PNG." });
        }

        var dto = new CrearPagoDto(
            form.EventoId,
            form.TipoPagoId,
            form.MetodoPagoId,
            form.Monto,
            form.FechaPago,
            form.Observacion,
            urlComprobante
        );

        var result = await pagoService.CrearAsync(dto, form.UsuarioId);
        return result
            ? Ok(new { mensaje = "Pago registrado correctamente." })
            : BadRequest(new { mensaje = "No se pudo registrar el pago. Verifique que el evento exista y que el monto no supere el saldo pendiente." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await pagoService.EliminarAsync(id);
        return result
            ? Ok(new { mensaje = "Pago eliminado correctamente." })
            : NotFound(new { mensaje = "Pago no encontrado." });
    }
}

public class CrearPagoForm
{
    public int EventoId { get; set; }
    public int TipoPagoId { get; set; }
    public int MetodoPagoId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string? Observacion { get; set; }
    public int UsuarioId { get; set; }
    public IFormFile? Comprobante { get; set; }
}
