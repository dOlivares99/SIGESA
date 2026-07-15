using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class PagosController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _apiBase;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public PagosController(IRestProvider restProvider, IHttpClientFactory httpFactory, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _httpFactory = httpFactory;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/pagosapi");
            var raw = JsonSerializer.Deserialize<List<JsonElement>>(response, JsonOpts) ?? new();

            var pagos = raw.Select(p => new PagoViewModel
            {
                PagoId = p.GetProperty("pagoId").GetInt32(),
                EventoId = p.GetProperty("eventoId").GetInt32(),
                Monto = p.GetProperty("monto").GetDecimal(),
                FechaPago = p.GetProperty("fechaPago").GetDateTime(),
                Observacion = p.TryGetProperty("observacion", out var o) && o.ValueKind != JsonValueKind.Null
                    ? o.GetString() : null,
                UrlComprobante = p.TryGetProperty("urlComprobante", out var u) && u.ValueKind != JsonValueKind.Null
                    ? u.GetString() : null,
                TipoPagoNombre = p.TryGetProperty("tipoPago", out var t) && t.ValueKind != JsonValueKind.Null
                    ? t.GetProperty("nombre").GetString() ?? "" : "",
                MetodoPagoNombre = p.TryGetProperty("metodoPago", out var m) && m.ValueKind != JsonValueKind.Null
                    ? m.GetProperty("nombre").GetString() ?? "" : "",
                ClienteNombre = p.TryGetProperty("evento", out var e) && e.ValueKind != JsonValueKind.Null
                    && e.TryGetProperty("cliente", out var c) && c.ValueKind != JsonValueKind.Null
                    ? c.GetProperty("nombre").GetString() ?? "" : "",
                TipoEvento = p.TryGetProperty("evento", out var ev) && ev.ValueKind != JsonValueKind.Null
                    ? ev.GetProperty("tipoEvento").GetString() ?? "" : "",
            }).ToList();

            return View(pagos);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar pagos: " + ex.Message;
            return View(new List<PagoViewModel>());
        }
    }

    public async Task<IActionResult> Crear()
    {
        var model = new PagoFormViewModel { FechaPago = DateTime.Today };
        await CargarListas(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PagoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarListas(model);
            return View(model);
        }

        try
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId") ?? "0";

            using var content = new MultipartFormDataContent
            {
                { new StringContent(model.EventoId.ToString()),     "EventoId" },
                { new StringContent(model.TipoPagoId.ToString()),   "TipoPagoId" },
                { new StringContent(model.MetodoPagoId.ToString()), "MetodoPagoId" },
                { new StringContent(model.Monto.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Monto" },
                { new StringContent(model.FechaPago.ToString("yyyy-MM-dd")), "FechaPago" },
                { new StringContent(usuarioId), "UsuarioId" }
            };

            if (!string.IsNullOrWhiteSpace(model.Observacion))
                content.Add(new StringContent(model.Observacion), "Observacion");

            if (model.Comprobante is { Length: > 0 })
            {
                var fileContent = new StreamContent(model.Comprobante.OpenReadStream());
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(model.Comprobante.ContentType);
                content.Add(fileContent, "Comprobante", model.Comprobante.FileName);
            }

            var client = _httpFactory.CreateClient();
            var response = await client.PostAsync(_apiBase + "/pagosapi", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                string mensaje = "Error al registrar el pago.";
                try
                {
                    var json = JsonSerializer.Deserialize<JsonElement>(body, JsonOpts);
                    if (json.TryGetProperty("mensaje", out var msg))
                        mensaje = msg.GetString() ?? mensaje;
                }
                catch { }
                ModelState.AddModelError("", mensaje);
                await CargarListas(model);
                return View(model);
            }

            TempData["Exito"] = "Pago registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al registrar el pago: " + ex.Message);
            await CargarListas(model);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            await _restProvider.DeleteAsync(_apiBase + "/pagosapi/" + id);
            TempData["Exito"] = "Pago eliminado correctamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al eliminar el pago: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListas(PagoFormViewModel model)
    {
        try
        {
            var resp = await _restProvider.GetAsync(_apiBase + "/reservasapi");
            model.Eventos = JsonSerializer.Deserialize<List<JsonElement>>(resp, JsonOpts)?
                .Where(e =>
                {
                    var total = e.GetProperty("montoTotal").GetDecimal();
                    var pagado = e.GetProperty("montoPagado").GetDecimal();
                    var estado = e.GetProperty("estado").GetString();
                    return total - pagado > 0 && estado != "Cancelada";
                })
                .Select(e => new EventoOpcionViewModel
                {
                    EventoId = e.GetProperty("eventoId").GetInt32(),
                    TipoEvento = e.GetProperty("tipoEvento").GetString() ?? "",
                    ClienteNombre = e.TryGetProperty("cliente", out var c) && c.ValueKind != JsonValueKind.Null
                        ? c.GetProperty("nombre").GetString() ?? "" : "",
                    SaldoPendiente = e.GetProperty("montoTotal").GetDecimal() - e.GetProperty("montoPagado").GetDecimal()
                }).ToList() ?? new();
        }
        catch { model.Eventos = new(); }

        try
        {
            var resp = await _restProvider.GetAsync(_apiBase + "/pagosapi/tipos");
            model.TiposPago = JsonSerializer.Deserialize<List<JsonElement>>(resp, JsonOpts)?
                .Select(t => new CatalogoOpcionViewModel
                {
                    Id = t.GetProperty("tipoPagoId").GetInt32(),
                    Nombre = t.GetProperty("nombre").GetString() ?? ""
                }).ToList() ?? new();
        }
        catch { model.TiposPago = new(); }

        try
        {
            var resp = await _restProvider.GetAsync(_apiBase + "/pagosapi/metodos");
            model.MetodosPago = JsonSerializer.Deserialize<List<JsonElement>>(resp, JsonOpts)?
                .Select(m => new CatalogoOpcionViewModel
                {
                    Id = m.GetProperty("metodoPagoId").GetInt32(),
                    Nombre = m.GetProperty("nombre").GetString() ?? ""
                }).ToList() ?? new();
        }
        catch { model.MetodosPago = new(); }
    }
}
