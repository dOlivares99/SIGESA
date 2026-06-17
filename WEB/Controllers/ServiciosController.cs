using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class ServiciosController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public ServiciosController(IRestProvider restProvider, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    // GET /Servicios
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/serviciosapi");
            var servicios = JsonSerializer.Deserialize<List<ServicioViewModel>>(response, JsonOpts)
                            ?? new();

            // Agrupar por categoría para mostrarlos ordenados en la vista
            ViewBag.Categorias = servicios
                .Select(s => s.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return View(servicios);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar servicios: " + ex.Message;
            return View(new List<ServicioViewModel>());
        }
    }

    // GET /Servicios/Crear
    public IActionResult Crear() => View(new ServicioFormViewModel());

    // POST /Servicios/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ServicioFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Nombre,
                model.Categoria,
                model.PrecioUnitario
            });
            await _restProvider.PostAsync(_apiBase + "/serviciosapi", json);
            TempData["Exito"] = "Servicio registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("409"))
        {
            ModelState.AddModelError("Nombre", "Ya existe un servicio con ese nombre.");
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al registrar servicio: " + ex.Message);
            return View(model);
        }
    }

    // GET /Servicios/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/serviciosapi/" + id);
            var s = JsonSerializer.Deserialize<ServicioViewModel>(response, JsonOpts);
            if (s == null) return NotFound();

            return View(new ServicioFormViewModel
            {
                ServicioId = s.ServicioId,
                Nombre = s.Nombre,
                Categoria = s.Categoria,
                PrecioUnitario = s.PrecioUnitario
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al cargar servicio: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST /Servicios/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ServicioFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Nombre,
                model.Categoria,
                model.PrecioUnitario
            });
            await _restProvider.PutAsync(_apiBase + "/serviciosapi/" + id, json);
            TempData["Exito"] = "Servicio actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("409"))
        {
            ModelState.AddModelError("Nombre", "Ya existe otro servicio con ese nombre.");
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al actualizar servicio: " + ex.Message);
            return View(model);
        }
    }

    // POST /Servicios/CambiarActivo
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarActivo(int id, bool activo)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { activo });
            await _restProvider.PatchAsync(_apiBase + $"/serviciosapi/{id}/activo", json);
            TempData["Exito"] = activo ? "Servicio activado." : "Servicio desactivado.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al cambiar estado: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}