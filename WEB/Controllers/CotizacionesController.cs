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
        new() { PropertyNameCaseInsensitive = true };

    public CotizacionesController(IRestProvider restProvider, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    // GET /Cotizaciones
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/cotizacionesapi");
            var raw = JsonSerializer.Deserialize<List<JsonElement>>(response, JsonOpts) ?? new();

            var cotizaciones = raw.Select(e => new CotizacionViewModel
            {
                CotizacionId = e.GetProperty("cotizacionId").GetInt32(),
                EventoId = e.GetProperty("eventoId").GetInt32(),
                Total = e.GetProperty("total").GetDecimal(),
                Estado = e.GetProperty("estado").GetString() ?? "",
                FechaCreacion = e.GetProperty("fechaCreacion").GetDateTime(),
                FechaVencimiento = e.TryGetProperty("fechaVencimiento", out var fv) &&
                                   fv.ValueKind != JsonValueKind.Null
                    ? fv.GetDateTime() : null,
                ClienteNombre = e.TryGetProperty("evento", out var ev) &&
                                ev.ValueKind != JsonValueKind.Null &&
                                ev.TryGetProperty("cliente", out var cl) &&
                                cl.ValueKind != JsonValueKind.Null
                    ? cl.GetProperty("nombre").GetString() ?? "" : "",
                TipoEvento = e.TryGetProperty("evento", out var ev2) &&
                                ev2.ValueKind != JsonValueKind.Null
                    ? ev2.GetProperty("tipoEvento").GetString() ?? "" : "",
                PaqueteNombre = e.TryGetProperty("evento", out var ev3) &&
                                ev3.ValueKind != JsonValueKind.Null &&
                                ev3.TryGetProperty("paquete", out var pk) &&
                                pk.ValueKind != JsonValueKind.Null
                    ? pk.GetProperty("nombre").GetString() ?? "" : "",
                UsuarioNombre = e.TryGetProperty("usuarioCreacionNavigation", out var u) &&
                                u.ValueKind != JsonValueKind.Null
                    ? u.GetProperty("nombre").GetString() ?? "" : "",
            }).ToList();

            return View(cotizaciones);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar cotizaciones: " + ex.Message;
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
    public async Task<IActionResult> Crear(CotizacionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarReservas(model);
            return View(model);
        }
        try
        {
            var usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId") ?? "0");
            var json = JsonSerializer.Serialize(new
            {
                model.EventoId,
                model.FechaVencimiento,
                UsuarioId = usuarioId
            });
            await _restProvider.PostAsync(_apiBase + "/cotizacionesapi", json);
            TempData["Exito"] = "Cotización creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al crear cotización: " + ex.Message);
            await CargarReservas(model);
            return View(model);
        }
    }

    // GET /Cotizaciones/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/cotizacionesapi/" + id);
            var c = JsonSerializer.Deserialize<JsonElement>(response, JsonOpts);

            var model = new CotizacionFormViewModel
            {
                CotizacionId = c.GetProperty("cotizacionId").GetInt32(),
                EventoId = c.GetProperty("eventoId").GetInt32(),
                Total = c.GetProperty("total").GetDecimal(),
                Estado = c.GetProperty("estado").GetString() ?? "Borrador",
                MotivoRechazo = c.TryGetProperty("motivoRechazo", out var mr) &&
                                   mr.ValueKind != JsonValueKind.Null
                    ? mr.GetString() : null,
                FechaVencimiento = c.TryGetProperty("fechaVencimiento", out var fv) &&
                                   fv.ValueKind != JsonValueKind.Null
                    ? fv.GetDateTime() : null,
            };
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al cargar cotización: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST /Cotizaciones/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CotizacionFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Estado,
                model.MotivoRechazo,
                model.Total,
                model.FechaVencimiento
            });
            await _restProvider.PutAsync(_apiBase + "/cotizacionesapi/" + id, json);
            TempData["Exito"] = "Cotización actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al actualizar cotización: " + ex.Message);
            return View(model);
        }
    }

    // Helper
    private async Task CargarReservas(CotizacionFormViewModel model)
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/reservasapi");
            var raw = JsonSerializer.Deserialize<List<JsonElement>>(response, JsonOpts) ?? new();

            model.Reservas = raw
                .Where(e => e.GetProperty("estado").GetString() != "Cancelada")
                .Select(e => new ReservaOpcionViewModel
                {
                    EventoId = e.GetProperty("eventoId").GetInt32(),
                    TipoEvento = e.GetProperty("tipoEvento").GetString() ?? "",
                    FechaEvento = e.GetProperty("fechaEvento").GetDateTime(),
                    MontoTotal = e.GetProperty("montoTotal").GetDecimal(),
                    ClienteNombre = e.TryGetProperty("cliente", out var c) &&
                                    c.ValueKind != JsonValueKind.Null
                        ? c.GetProperty("nombre").GetString() ?? "" : "",
                }).ToList();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorReservas = "Error cargando reservas: " + ex.Message;
            model.Reservas = new();
        }
    }
}