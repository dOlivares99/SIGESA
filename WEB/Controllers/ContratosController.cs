using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class ContratosController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    private static readonly JsonSerializerOptions JsonOpts =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public ContratosController(
        IRestProvider restProvider,
        IConfiguration configuration)
    {
        _restProvider = restProvider;

        _apiBase =
            configuration["ApiSettings:BaseUrl"]
            ?? "http://localhost:5119/api";
    }

    // GET: /Contratos
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(
                _apiBase + "/contratosapi");

            var raw =
                JsonSerializer.Deserialize<List<JsonElement>>(
                    response,
                    JsonOpts)
                ?? new List<JsonElement>();

            var contratos = raw
                .Select(MapearContrato)
                .ToList();

            return View(contratos);
        }
        catch (Exception ex)
        {
            ViewBag.Error =
                "Error al cargar los contratos: " + ex.Message;

            return View(new List<ContratoViewModel>());
        }
    }

    // GET: /Contratos/Crear
    public async Task<IActionResult> Crear(int? cotizacionId)
    {
        var model = new ContratoFormViewModel
        {
            Estado = "Pendiente"
        };

        await CargarCotizaciones(model);

        if (cotizacionId.HasValue && cotizacionId.Value > 0)
        {
            model.CotizacionId = cotizacionId.Value;
        }

        return View(model);
    }

    // POST: /Contratos/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        ContratoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCotizaciones(model);
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

                await CargarCotizaciones(model);

                return View(model);
            }

            var json = JsonSerializer.Serialize(new
            {
                model.CotizacionId,
                model.Observaciones,
                UsuarioId = usuarioId
            });

            await _restProvider.PostAsync(
                _apiBase + "/contratosapi",
                json);

            TempData["Exito"] =
                "Contrato creado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                "",
                "Error al crear el contrato: " + ex.Message);

            await CargarCotizaciones(model);

            return View(model);
        }
    }

    // GET: /Contratos/Detalles/5
    public async Task<IActionResult> Detalles(int id)
    {
        try
        {
            var response = await _restProvider.GetAsync(
                _apiBase + "/contratosapi/" + id);

            var contratoJson =
                JsonSerializer.Deserialize<JsonElement>(
                    response,
                    JsonOpts);

            var model = MapearContrato(contratoJson);

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                "Error al cargar el contrato: " + ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Contratos/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var response = await _restProvider.GetAsync(
                _apiBase + "/contratosapi/" + id);

            var contrato =
                JsonSerializer.Deserialize<JsonElement>(
                    response,
                    JsonOpts);

            var model = new ContratoFormViewModel
            {
                ContratoId =
                    ObtenerEntero(
                        contrato,
                        "contratoId"),

                CotizacionId =
                    ObtenerEntero(
                        contrato,
                        "cotizacionId"),

                NumeroContrato =
                    ObtenerTexto(
                        contrato,
                        "numeroContrato"),

                Estado =
                    ObtenerTexto(
                        contrato,
                        "estado",
                        "Pendiente"),

                Observaciones =
                    ObtenerTextoNulo(
                        contrato,
                        "observaciones"),

                FechaContrato =
                    ObtenerFecha(
                        contrato,
                        "fechaContrato")
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                "Error al cargar el contrato: " + ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Contratos/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        ContratoFormViewModel model)
    {
        model.ContratoId = id;

        ModelState.Remove(nameof(model.CotizacionId));

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Estado,
                model.Observaciones
            });

            await _restProvider.PutAsync(
                _apiBase + "/contratosapi/" + id,
                json);

            TempData["Exito"] =
                "Contrato actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                "",
                "Error al actualizar el contrato: " + ex.Message);

            return View(model);
        }
    }

    // POST: /Contratos/Enviar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enviar(int id)
    {
        try
        {
            await _restProvider.PostAsync(
                _apiBase + $"/contratosapi/{id}/enviar",
                "{}");

            TempData["Exito"] =
                "El contrato fue enviado correctamente al cliente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                "No se pudo enviar el contrato: " + ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Contratos/DescargarPdf/5
    public IActionResult DescargarPdf(int id)
    {
        try
        {
            var url =
                _apiBase + $"/contratosapi/{id}/pdf";

            return Redirect(url);
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                "No se pudo descargar el contrato: " + ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }

    private async Task CargarCotizaciones(
        ContratoFormViewModel model)
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

            model.Cotizaciones = raw
                .Where(CotizacionPuedeGenerarContrato)
                .Select(cotizacion =>
                {
                    var evento =
                        ObtenerObjeto(
                            cotizacion,
                            "evento");

                    var cliente =
                        ObtenerObjeto(
                            evento,
                            "cliente");

                    return new CotizacionContratoOpcionViewModel
                    {
                        CotizacionId =
                            ObtenerEntero(
                                cotizacion,
                                "cotizacionId"),

                        Estado =
                            ObtenerTexto(
                                cotizacion,
                                "estado"),

                        Total =
                            ObtenerDecimal(
                                cotizacion,
                                "total"),

                        ClienteNombre =
                            ObtenerTexto(
                                cliente,
                                "nombre",
                                "Cliente no registrado"),

                        TipoEvento =
                            ObtenerTexto(
                                evento,
                                "tipoEvento",
                                "Evento"),

                        FechaEvento =
                            ObtenerFecha(
                                evento,
                                "fechaEvento")
                    };
                })
                .OrderByDescending(c => c.FechaEvento)
                .ToList();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorCotizaciones =
                "Error cargando cotizaciones: " + ex.Message;

            model.Cotizaciones =
                new List<CotizacionContratoOpcionViewModel>();
        }
    }

    private static bool CotizacionPuedeGenerarContrato(
        JsonElement cotizacion)
    {
        var estado =
            ObtenerTexto(
                cotizacion,
                "estado");

        var aceptada =
            estado.Equals(
                "Aceptada",
                StringComparison.OrdinalIgnoreCase);

        var aprobada =
            estado.Equals(
                "Aprobada",
                StringComparison.OrdinalIgnoreCase);

        if (!aceptada && !aprobada)
        {
            return false;
        }

        if (cotizacion.TryGetProperty(
                "contrato",
                out var contrato)
            && contrato.ValueKind != JsonValueKind.Null
            && contrato.ValueKind != JsonValueKind.Undefined)
        {
            return false;
        }

        return true;
    }

    private static ContratoViewModel MapearContrato(
        JsonElement contrato)
    {
        var cotizacion =
            ObtenerObjeto(
                contrato,
                "cotizacion");

        var evento =
            ObtenerObjeto(
                cotizacion,
                "evento");

        var cliente =
            ObtenerObjeto(
                evento,
                "cliente");

        var paquete =
            ObtenerObjeto(
                evento,
                "paquete");

        var usuario =
            ObtenerObjeto(
                contrato,
                "usuarioCreacionNavigation");

        return new ContratoViewModel
        {
            ContratoId =
                ObtenerEntero(
                    contrato,
                    "contratoId"),

            CotizacionId =
                ObtenerEntero(
                    contrato,
                    "cotizacionId"),

            NumeroContrato =
                ObtenerTexto(
                    contrato,
                    "numeroContrato"),

            FechaContrato =
                ObtenerFecha(
                    contrato,
                    "fechaContrato"),

            Estado =
                ObtenerTexto(
                    contrato,
                    "estado"),

            Observaciones =
                ObtenerTextoNulo(
                    contrato,
                    "observaciones"),

            RutaPdf =
                ObtenerTextoNulo(
                    contrato,
                    "rutaPdf"),

            ClienteNombre =
                ObtenerTexto(
                    cliente,
                    "nombre"),

            ClienteCorreo =
                ObtenerTexto(
                    cliente,
                    "email"),

            TipoEvento =
                ObtenerTexto(
                    evento,
                    "tipoEvento"),

            FechaEvento =
                ObtenerFecha(
                    evento,
                    "fechaEvento"),

            PaqueteNombre =
                ObtenerTexto(
                    paquete,
                    "nombre"),

            Total =
                ObtenerDecimal(
                    cotizacion,
                    "total"),

            UsuarioNombre =
                ObtenerTexto(
                    usuario,
                    "nombre")
        };
    }

    private static JsonElement ObtenerObjeto(
        JsonElement elemento,
        string propiedad)
    {
        if (elemento.ValueKind == JsonValueKind.Object
            && elemento.TryGetProperty(
                propiedad,
                out var resultado)
            && resultado.ValueKind == JsonValueKind.Object)
        {
            return resultado;
        }

        return default;
    }

    private static string ObtenerTexto(
        JsonElement elemento,
        string propiedad,
        string valorPredeterminado = "")
    {
        if (elemento.ValueKind == JsonValueKind.Object
            && elemento.TryGetProperty(
                propiedad,
                out var resultado)
            && resultado.ValueKind == JsonValueKind.String)
        {
            return resultado.GetString()
                   ?? valorPredeterminado;
        }

        return valorPredeterminado;
    }

    private static string? ObtenerTextoNulo(
        JsonElement elemento,
        string propiedad)
    {
        if (elemento.ValueKind == JsonValueKind.Object
            && elemento.TryGetProperty(
                propiedad,
                out var resultado)
            && resultado.ValueKind == JsonValueKind.String)
        {
            return resultado.GetString();
        }

        return null;
    }

    private static int ObtenerEntero(
        JsonElement elemento,
        string propiedad)
    {
        if (elemento.ValueKind == JsonValueKind.Object
            && elemento.TryGetProperty(
                propiedad,
                out var resultado)
            && resultado.ValueKind == JsonValueKind.Number
            && resultado.TryGetInt32(out var valor))
        {
            return valor;
        }

        return 0;
    }

    private static decimal ObtenerDecimal(
        JsonElement elemento,
        string propiedad)
    {
        if (elemento.ValueKind == JsonValueKind.Object
            && elemento.TryGetProperty(
                propiedad,
                out var resultado)
            && resultado.ValueKind == JsonValueKind.Number
            && resultado.TryGetDecimal(out var valor))
        {
            return valor;
        }

        return 0;
    }

    private static DateTime ObtenerFecha(
        JsonElement elemento,
        string propiedad)
    {
        if (elemento.ValueKind == JsonValueKind.Object
            && elemento.TryGetProperty(
                propiedad,
                out var resultado)
            && resultado.ValueKind == JsonValueKind.String
            && resultado.TryGetDateTime(out var fecha))
        {
            return fecha;
        }

        return DateTime.MinValue;
    }
}