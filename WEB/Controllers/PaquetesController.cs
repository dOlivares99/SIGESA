using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class PaquetesController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public PaquetesController(IRestProvider restProvider, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    // GET /Paquetes
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/paquetesapi");
            var paquetes = JsonSerializer.Deserialize<List<PaqueteViewModel>>(response, JsonOpts)
                           ?? new();
            return View(paquetes);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar paquetes: " + ex.Message;
            return View(new List<PaqueteViewModel>());
        }
    }

    // GET /Paquetes/Crear
    public IActionResult Crear() => View(new PaqueteFormViewModel());

    // POST /Paquetes/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PaqueteFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Nombre,
                model.Descripcion,
                model.PrecioBase,
                model.MaxPersonas,
                model.DuracionHoras
            });
            await _restProvider.PostAsync(_apiBase + "/paquetesapi", json);
            TempData["Exito"] = "Paquete registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("409"))
        {
            ModelState.AddModelError("Nombre", "Ya existe un paquete con ese nombre.");
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al registrar paquete: " + ex.Message);
            return View(model);
        }
    }

    // GET /Paquetes/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/paquetesapi/" + id);
            var p = JsonSerializer.Deserialize<PaqueteViewModel>(response, JsonOpts);
            if (p == null) return NotFound();

            return View(new PaqueteFormViewModel
            {
                PaqueteId = p.PaqueteId,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioBase = p.PrecioBase,
                MaxPersonas = p.MaxPersonas,
                DuracionHoras = p.DuracionHoras
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al cargar paquete: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST /Paquetes/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, PaqueteFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Nombre,
                model.Descripcion,
                model.PrecioBase,
                model.MaxPersonas,
                model.DuracionHoras
            });
            var response = await _restProvider.PutAsync(_apiBase + "/paquetesapi/" + id, json);

            // Leer advertencia de cotizaciones en Borrador (escenario 2 edición)
            var result = JsonSerializer.Deserialize<JsonElement>(response, JsonOpts);
            if (result.TryGetProperty("advertencia", out var adv) &&
                adv.ValueKind != JsonValueKind.Null)
            {
                TempData["Advertencia"] = adv.GetString();
            }

            TempData["Exito"] = "Paquete actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("409"))
        {
            ModelState.AddModelError("Nombre", "Ya existe otro paquete con ese nombre.");
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al actualizar paquete: " + ex.Message);
            return View(model);
        }
    }

    // POST /Paquetes/CambiarActivo
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarActivo(int id, bool activo)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { activo });
            await _restProvider.PutAsync(_apiBase + $"/paquetesapi/{id}/activo", json);
            TempData["Exito"] = activo ? "Paquete activado." : "Paquete desactivado.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al cambiar estado: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}