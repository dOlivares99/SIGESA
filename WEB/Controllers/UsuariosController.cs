using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class UsuariosController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    public UsuariosController(IRestProvider restProvider, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/usuariosapi");
            var usuarios = JsonSerializer.Deserialize<List<UsuarioViewModel>>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<UsuarioViewModel>();

            // Mapear RolNombre desde el objeto Rol anidado
            foreach (var u in usuarios)
                u.RolNombre = u.Rol?.Nombre ?? string.Empty;

            return View(usuarios);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar usuarios: " + ex.Message;
            return View(new List<UsuarioViewModel>());
        }
    }

    public IActionResult Crear() => View(new CrearUsuarioViewModel());

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
            await _restProvider.PostAsync(_apiBase + "/usuariosapi", json);
            TempData["Exito"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al crear usuario: " + ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/usuariosapi/" + id);
            var usuario = JsonSerializer.Deserialize<UsuarioViewModel>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (usuario == null) return NotFound();
            usuario.RolNombre = usuario.Rol?.Nombre ?? string.Empty;
            return View(usuario);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar usuario: " + ex.Message;
            return NotFound();
        }
    }

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
            await _restProvider.PutAsync(_apiBase + "/usuariosapi/" + id, json);
            TempData["Exito"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al actualizar usuario: " + ex.Message);
            return View(model);
        }
    }
}
