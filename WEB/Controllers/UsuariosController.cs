using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class UsuariosController(IRestProvider restProvider, IConfiguration configuration) : Controller
{
    private readonly string _apiBase = configuration["ApiSettings:BaseUrl"]
        ?? "https://localhost:7001/api";

    // GET /Usuarios
    public async Task<IActionResult> Index()
    {
        try
        {
            var response  = await restProvider.GetAsync($"{_apiBase}/usuariosapi");
            var usuarios  = JsonSerializer.Deserialize<List<UsuarioViewModel>>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<UsuarioViewModel>();
            return View(usuarios);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Error al cargar usuarios: {ex.Message}";
            return View(new List<UsuarioViewModel>());
        }
    }

    // GET /Usuarios/Crear
    public IActionResult Crear() => View(new CrearUsuarioViewModel());

    // POST /Usuarios/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearUsuarioViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Nombre,
                model.Email,
                model.Password,
                model.RolId
            });
            await restProvider.PostAsync($"{_apiBase}/usuariosapi", json);
            TempData["Exito"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error al crear usuario: {ex.Message}");
            return View(model);
        }
    }

    // GET /Usuarios/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var response = await restProvider.GetAsync($"{_apiBase}/usuariosapi/{id}");
            var usuario  = JsonSerializer.Deserialize<UsuarioViewModel>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (usuario == null) return NotFound();
            return View(usuario);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Error al cargar usuario: {ex.Message}";
            return NotFound();
        }
    }

    // POST /Usuarios/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, UsuarioViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Nombre,
                model.Email,
                model.RolId
            });
            await restProvider.PutAsync($"{_apiBase}/usuariosapi/{id}", json);
            TempData["Exito"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error al actualizar usuario: {ex.Message}");
            return View(model);
        }
    }
}
