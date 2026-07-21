using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class CotizacionesController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    private static readonly JsonSerializerOptions JsonOpts =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public CotizacionesController(
        IRestProvider restProvider,
        IConfiguration configuration)
    {
        _restProvider = restProvider;

        _apiBase =
            configuration["ApiSettings:BaseUrl"]
            ?? "http://localhost:5119/api";
    }

    // GET /Cotizaciones
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(
                _apiBase + "/cotizacionesapi");

            var raw =
                JsonSerializer.Deserialize<List<JsonElement>>(
                    response,
                    JsonOpts)
                ?? new List<JsonElement>();

            var cotizaciones = raw
                .Select(e => new CotizacionViewModel
                {
                    CotizacionId =
                        e.GetProperty("cotizacionId").GetInt32(),

                    EventoId =
                        e.GetProperty("eventoId").GetInt32(),

                    Total =
                        e.GetProperty("total").GetDecimal(),

                    Estado =
                        e.GetProperty("estado").GetString() ?? "",

                    FechaCreacion =
                        e.GetProperty("fechaCreacion").GetDateTime(),

                    FechaVencimiento =
                        e.TryGetProperty(
                            "fechaVencimiento",
                            out var fechaVencimiento)
                        && fechaVencimiento.ValueKind
                            != JsonValueKind.Null
                            ? fechaVencimiento.GetDateTime()
                            : null,

                    ClienteNombre =
                        e.TryGetProperty(
                            "evento",
                            out var evento)
                        && evento.ValueKind != JsonValueKind.Null
                        && evento.TryGetProperty(
                            "cliente",
                            out var cliente)
                        && cliente.ValueKind != JsonValueKind.Null
                        && cliente.TryGetProperty(
                            "nombre",
                            out var nombreCliente)
                            ? nombreCliente.GetString() ?? ""
                            : "",

                    TipoEvento =
                        e.TryGetProperty(
                            "evento",
                            out var eventoTipo)
                        && eventoTipo.ValueKind != JsonValueKind.Null
                        && eventoTipo.TryGetProperty(
                            "tipoEvento",
                            out var tipoEvento)
                            ? tipoEvento.GetString() ?? ""
                            : "",

                    PaqueteNombre =
                        e.TryGetProperty(
                            "evento",
                            out var eventoPaquete)
                        && eventoPaquete.ValueKind
                            != JsonValueKind.Null
                        && eventoPaquete.TryGetProperty(
                            "paquete",
                            out var paquete)
                        && paquete.ValueKind != JsonValueKind.Null
                        && paquete.TryGetProperty(
                            "nombre",
                            out var nombrePaquete)
                            ? nombrePaquete.GetString() ?? ""
                            : "",

                    UsuarioNombre =
                        e.TryGetProperty(
                            "usuarioCreacionNavigation",
                            out var usuario)
                        && usuario.ValueKind != JsonValueKind.Null
                        && usuario.TryGetProperty(
                            "nombre",
                            out var nombreUsuario)
                            ? nombreUsuario.GetString() ?? ""
                            : "",

                    TieneContrato =
                        e.TryGetProperty(
                            "tieneContrato",
                            out var tieneContrato)
                        && tieneContrato.ValueKind
                            == JsonValueKind.True,

                    ContratoId =
                        e.TryGetProperty(
                            "contratoId",
                            out var contratoId)
                        && contratoId.ValueKind
                            == JsonValueKind.Number
                            ? contratoId.GetInt32()
                            : null
                })
                .ToList();

            return View(cotizaciones);
        }
        catch (Exception ex)
        {
            ViewBag.Error =
                "Error al cargar cotizaciones: " + ex.Message;

            return View(new List<CotizacionViewModel>());
        }
    }

    // GET /Cotizaciones/Crear
    public async Task<IActionResult> Crear()
    {
        var model = new CotizacionFormViewModel();

        await CargarReservas(model);

        return View(model);
    }

    // POST /Cotizaciones/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        CotizacionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarReservas(model);

            return View(model);
        }

        try
        {
            var usuarioIdTexto =
                HttpContext.Session.GetString("UsuarioId");

            if (!int.TryParse(
                    usuarioIdTexto,
                    out var usuarioId)
                || usuarioId <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo identificar al usuario de la sesión.");

                await CargarReservas(model);

                return View(model);
            }

            var json = JsonSerializer.Serialize(new
            {
                model.EventoId,
                model.FechaVencimiento,
                UsuarioId = usuarioId
            });

            await _restProvider.PostAsync(
                _apiBase + "/cotizacionesapi",
                json);

            TempData["Exito"] =
                "Cotización creada correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                "",
                "Error al crear cotización: " + ex.Message);

            await CargarReservas(model);

            return View(model);
        }
    }

    // GET /Cotizaciones/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var response = await _restProvider.GetAsync(
                _apiBase + "/cotizacionesapi/" + id);

            var cotizacion =
                JsonSerializer.Deserialize<JsonElement>(
                    response,
                    JsonOpts);

            var model = new CotizacionFormViewModel
            {
                CotizacionId =
                    cotizacion
                        .GetProperty("cotizacionId")
                        .GetInt32(),

                EventoId =
                    cotizacion
                        .GetProperty("eventoId")
                        .GetInt32(),

                Total =
                    cotizacion
                        .GetProperty("total")
                        .GetDecimal(),

                Estado =
                    cotizacion
                        .GetProperty("estado")
                        .GetString()
                    ?? "Borrador",

                MotivoRechazo =
                    cotizacion.TryGetProperty(
                        "motivoRechazo",
                        out var motivoRechazo)
                    && motivoRechazo.ValueKind
                        != JsonValueKind.Null
                        ? motivoRechazo.GetString()
                        : null,

                FechaVencimiento =
                    cotizacion.TryGetProperty(
                        "fechaVencimiento",
                        out var fechaVencimiento)
                    && fechaVencimiento.ValueKind
                        != JsonValueKind.Null
                        ? fechaVencimiento.GetDateTime()
                        : null
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                "Error al cargar cotización: " + ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }

    // POST /Cotizaciones/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        CotizacionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Estado,
                model.MotivoRechazo,
                model.Total,
                model.FechaVencimiento
            });

            await _restProvider.PutAsync(
                _apiBase + "/cotizacionesapi/" + id,
                json);

            TempData["Exito"] =
                "Cotización actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                "",
                "Error al actualizar cotización: " + ex.Message);

            return View(model);
        }
    }

    // POST /Cotizaciones/Enviar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enviar(int id)
    {
        try
        {
            await _restProvider.PostAsync(
                _apiBase + $"/cotizacionesapi/{id}/enviar",
                "{}");

            TempData["Exito"] =
                $"La cotización #{id} fue enviada correctamente al cliente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                $"No se pudo enviar la cotización #{id}: {ex.Message}";

            return RedirectToAction(nameof(Index));
        }
    }

    // GET /Cotizaciones/DescargarPdf/5
    public async Task<IActionResult> DescargarPdf(int id)
    {
        try
        {
            var url =
                _apiBase + $"/cotizacionesapi/{id}/pdf";

            return Redirect(url);
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                $"No se pudo descargar la cotización #{id}: {ex.Message}";

            return RedirectToAction(nameof(Index));
        }
    }

    // Helper
    private async Task CargarReservas(
        CotizacionFormViewModel model)
    {
        try
        {
            var response = await _restProvider.GetAsync(
                _apiBase + "/reservasapi");

            var raw =
                JsonSerializer.Deserialize<List<JsonElement>>(
                    response,
                    JsonOpts)
                ?? new List<JsonElement>();

            model.Reservas = raw
                .Where(e =>
                    !e.TryGetProperty(
                        "estado",
                        out var estado)
                    || estado.GetString() != "Cancelada")
                .Select(e => new ReservaOpcionViewModel
                {
                    EventoId =
                        e.GetProperty("eventoId").GetInt32(),

                    TipoEvento =
                        e.GetProperty("tipoEvento").GetString()
                        ?? "",

                    FechaEvento =
                        e.GetProperty("fechaEvento").GetDateTime(),

                    MontoTotal =
                        e.GetProperty("montoTotal").GetDecimal(),

                    ClienteNombre =
                        e.TryGetProperty(
                            "cliente",
                            out var cliente)
                        && cliente.ValueKind != JsonValueKind.Null
                        && cliente.TryGetProperty(
                            "nombre",
                            out var nombreCliente)
                            ? nombreCliente.GetString() ?? ""
                            : ""
                })
                .ToList();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorReservas =
                "Error cargando reservas: " + ex.Message;

            model.Reservas =
                new List<ReservaOpcionViewModel>();
        }
    }
}