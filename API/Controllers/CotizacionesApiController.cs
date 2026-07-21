using API.Services;
using Business.Services;
using Microsoft.AspNetCore.Mvc;
using Models.Entities;
using System.Net.Mail;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CotizacionesApiController(
    ICotizacionService cotizacionService,
    ICotizacionPdfService cotizacionPdfService,
    IEmailService emailService) : ControllerBase
{
    // GET api/cotizacionesapi
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var cotizaciones =
            await cotizacionService.ObtenerTodasAsync();

        var respuesta = cotizaciones
            .Select(CrearRespuestaCotizacion)
            .ToList();

        return Ok(respuesta);
    }

    // GET api/cotizacionesapi/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var cotizacion =
            await cotizacionService.ObtenerPorIdAsync(id);

        if (cotizacion == null)
        {
            return NotFound(new
            {
                mensaje =
                    $"No se encontró la cotización #{id}."
            });
        }

        return Ok(CrearRespuestaCotizacion(cotizacion));
    }

    // POST api/cotizacionesapi
    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] CrearCotizacionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result =
            await cotizacionService.CrearAsync(
                request.EventoId,
                request.FechaVencimiento,
                request.UsuarioId);

        return result
            ? Ok(new
            {
                mensaje =
                    "Cotización creada correctamente."
            })
            : StatusCode(500, new
            {
                mensaje =
                    "Error al crear la cotización."
            });
    }

    // PUT api/cotizacionesapi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(
        int id,
        [FromBody] ActualizarCotizacionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result =
            await cotizacionService.ActualizarEstadoAsync(
                id,
                request.Estado,
                request.MotivoRechazo,
                request.Total,
                request.FechaVencimiento);

        return result
            ? Ok(new
            {
                mensaje =
                    "Cotización actualizada correctamente."
            })
            : NotFound(new
            {
                mensaje =
                    "Cotización no encontrada."
            });
    }

    // GET api/cotizacionesapi/5/pdf
    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        var cotizacion =
            await cotizacionService.ObtenerPorIdAsync(id);

        if (cotizacion == null)
        {
            return NotFound(new
            {
                mensaje =
                    $"No se encontró la cotización #{id}."
            });
        }

        var pdf =
            cotizacionPdfService.GenerarPdf(cotizacion);

        return File(
            pdf,
            "application/pdf",
            $"Cotizacion-{cotizacion.CotizacionId}.pdf");
    }

    // POST api/cotizacionesapi/5/enviar
    [HttpPost("{id}/enviar")]
    public async Task<IActionResult> EnviarCotizacion(int id)
    {
        try
        {
            var cotizacion =
                await cotizacionService.ObtenerPorIdAsync(id);

            if (cotizacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No se encontró la cotización #{id}."
                });
            }

            var evento =
                cotizacion.Evento;

            if (evento == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La cotización no tiene un evento asociado."
                });
            }

            var cliente =
                evento.Cliente;

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
                        "El cliente no tiene un correo electrónico registrado."
                });
            }

            var pdf =
                cotizacionPdfService.GenerarPdf(cotizacion);

            await emailService.EnviarCotizacionAsync(
                cliente.Email,
                cliente.Nombre,
                cotizacion.CotizacionId,
                evento.TipoEvento,
                evento.FechaEvento,
                cotizacion.Total,
                cotizacion.FechaVencimiento,
                pdf);

            var estadoActualizado =
                await cotizacionService.ActualizarEstadoAsync(
                    cotizacion.CotizacionId,
                    "Enviada",
                    null,
                    cotizacion.Total,
                    cotizacion.FechaVencimiento);

            if (!estadoActualizado)
            {
                return StatusCode(500, new
                {
                    mensaje =
                        "El correo fue enviado, pero no se pudo actualizar el estado de la cotización."
                });
            }

            return Ok(new
            {
                mensaje =
                    $"La cotización #{cotizacion.CotizacionId} fue enviada correctamente a {cliente.Email}.",

                estado = "Enviada"
            });
        }
        catch (SmtpException ex)
        {
            return StatusCode(500, new
            {
                mensaje =
                    "No se pudo enviar el correo de la cotización.",

                detalle = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                mensaje =
                    "Ocurrió un error al enviar la cotización.",

                detalle = ex.Message
            });
        }
    }

    private static object CrearRespuestaCotizacion(
        Cotizacion cotizacion)
    {
        return new
        {
            cotizacion.CotizacionId,
            cotizacion.EventoId,
            cotizacion.Total,
            cotizacion.Estado,
            cotizacion.MotivoRechazo,
            cotizacion.FechaCreacion,
            cotizacion.FechaVencimiento,

            tieneContrato =
                cotizacion.Contrato != null,

            contratoId =
                cotizacion.Contrato?.ContratoId,

            contrato = cotizacion.Contrato == null
                ? null
                : new
                {
                    cotizacion.Contrato.ContratoId,
                    cotizacion.Contrato.NumeroContrato,
                    cotizacion.Contrato.FechaContrato,
                    cotizacion.Contrato.Estado
                },

            evento = cotizacion.Evento == null
                ? null
                : new
                {
                    cotizacion.Evento.EventoId,
                    cotizacion.Evento.TipoEvento,
                    cotizacion.Evento.FechaEvento,
                    cotizacion.Evento.MontoTotal,
                    cotizacion.Evento.MontoPagado,
                    cotizacion.Evento.SaldoPendiente,

                    cliente = cotizacion.Evento.Cliente == null
                        ? null
                        : new
                        {
                            cotizacion.Evento.Cliente.ClienteId,
                            cotizacion.Evento.Cliente.Nombre,
                            cotizacion.Evento.Cliente.Email,
                            cotizacion.Evento.Cliente.Telefono
                        },

                    paquete = cotizacion.Evento.Paquete == null
                        ? null
                        : new
                        {
                            cotizacion.Evento.Paquete.PaqueteId,
                            cotizacion.Evento.Paquete.Nombre
                        }
                },

            usuarioCreacionNavigation =
                cotizacion.UsuarioCreacionNavigation == null
                    ? null
                    : new
                    {
                        cotizacion.UsuarioCreacionNavigation.UsuarioId,
                        cotizacion.UsuarioCreacionNavigation.Nombre
                    }
        };
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