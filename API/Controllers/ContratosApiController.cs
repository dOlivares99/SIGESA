using API.Services;
using Business.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContratosApiController : ControllerBase
{
    private readonly IContratoService _contratoService;
    private readonly IContratoPdfService _contratoPdfService;
    private readonly IEmailService _emailService;

    public ContratosApiController(
        IContratoService contratoService,
        IContratoPdfService contratoPdfService,
        IEmailService emailService)
    {
        _contratoService = contratoService;
        _contratoPdfService = contratoPdfService;
        _emailService = emailService;
    }

    // GET: api/ContratosApi
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        try
        {
            var contratos =
                await _contratoService.ObtenerTodosAsync();

            return Ok(contratos);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje = "Error al obtener los contratos.",
                    detalle = ex.Message
                });
        }
    }

    // GET: api/ContratosApi/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var contrato =
                await _contratoService.ObtenerPorIdAsync(id);

            if (contrato == null)
            {
                return NotFound(new
                {
                    mensaje = "El contrato no existe."
                });
            }

            return Ok(contrato);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje = "Error al obtener el contrato.",
                    detalle = ex.Message
                });
        }
    }

    // GET: api/ContratosApi/cotizacion/5
    [HttpGet("cotizacion/{cotizacionId:int}")]
    public async Task<IActionResult> ObtenerPorCotizacion(
        int cotizacionId)
    {
        try
        {
            var contrato =
                await _contratoService
                    .ObtenerPorCotizacionAsync(cotizacionId);

            if (contrato == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No existe un contrato para esta cotización."
                });
            }

            return Ok(contrato);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje =
                        "Error al obtener el contrato por cotización.",
                    detalle = ex.Message
                });
        }
    }

    // POST: api/ContratosApi
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearContratoRequest request)
    {
        try
        {
            if (request.CotizacionId <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Debe indicar una cotización válida."
                });
            }

            if (request.UsuarioId <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Debe indicar un usuario válido."
                });
            }

            var resultado =
                await _contratoService.CrearAsync(
                    request.CotizacionId,
                    request.Observaciones,
                    request.UsuarioId);

            if (!resultado.Exito)
            {
                return BadRequest(new
                {
                    mensaje = resultado.Mensaje
                });
            }

            return Ok(new
            {
                mensaje = resultado.Mensaje,
                contrato = resultado.Contrato
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje = "Error al crear el contrato.",
                    detalle = ex.Message
                });
        }
    }

    // PUT: api/ContratosApi/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] ActualizarContratoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Estado))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Debe indicar el estado del contrato."
                });
            }

            var actualizado =
                await _contratoService.ActualizarAsync(
                    id,
                    request.Estado.Trim(),
                    request.Observaciones);

            if (!actualizado)
            {
                return NotFound(new
                {
                    mensaje =
                        "No se encontró el contrato o no se pudo actualizar."
                });
            }

            return Ok(new
            {
                mensaje = "Contrato actualizado correctamente."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje = "Error al actualizar el contrato.",
                    detalle = ex.Message
                });
        }
    }

    // GET: api/ContratosApi/5/pdf
    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        try
        {
            var contrato =
                await _contratoService.ObtenerPorIdAsync(id);

            if (contrato == null)
            {
                return NotFound(new
                {
                    mensaje = "El contrato no existe."
                });
            }

            var pdf =
                _contratoPdfService.GenerarPdf(contrato);

            var nombreArchivo =
                $"Contrato-{contrato.NumeroContrato}.pdf";

            return File(
                pdf,
                "application/pdf",
                nombreArchivo);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje =
                        "Error al generar el PDF del contrato.",
                    detalle = ex.Message
                });
        }
    }

    // POST: api/ContratosApi/5/enviar
    [HttpPost("{id:int}/enviar")]
    public async Task<IActionResult> Enviar(int id)
    {
        try
        {
            var contrato =
                await _contratoService.ObtenerPorIdAsync(id);

            if (contrato == null)
            {
                return NotFound(new
                {
                    mensaje = "El contrato no existe."
                });
            }

            var cotizacion = contrato.Cotizacion;
            var evento = cotizacion?.Evento;
            var cliente = evento?.Cliente;

            if (cotizacion == null || evento == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El contrato no tiene una cotización o evento válido."
                });
            }

            if (cliente == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El evento no tiene un cliente asociado."
                });
            }

            if (string.IsNullOrWhiteSpace(cliente.Email))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El cliente no tiene un correo registrado."
                });
            }

            var pdf =
                _contratoPdfService.GenerarPdf(contrato);

            await _emailService.EnviarContratoAsync(
                cliente.Email,
                cliente.Nombre,
                contrato.NumeroContrato,
                evento.TipoEvento,
                evento.FechaEvento,
                cotizacion.Total,
                pdf);

            var actualizado =
                await _contratoService.ActualizarAsync(
                    contrato.ContratoId,
                    "Enviado",
                    contrato.Observaciones);

            if (!actualizado)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "El contrato se envió, pero no se pudo actualizar su estado."
                    });
            }

            return Ok(new
            {
                mensaje =
                    "Contrato enviado correctamente al cliente."
            });
        }
        catch (SmtpException ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje =
                        "No se pudo enviar el correo del contrato.",
                    detalle = ex.Message
                });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensaje = "Error al enviar el contrato.",
                    detalle = ex.Message
                });
        }
    }
}

public class CrearContratoRequest
{
    public int CotizacionId { get; set; }

    public string? Observaciones { get; set; }

    public int UsuarioId { get; set; }
}

public class ActualizarContratoRequest
{
    public string Estado { get; set; } = null!;

    public string? Observaciones { get; set; }
}