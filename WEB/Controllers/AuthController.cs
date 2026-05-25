using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class AuthController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    public AuthController(IRestProvider restProvider, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("IsLoggedIn") == "true")
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Email,
                model.Password
            });
            var response = await _restProvider.PostAsync(_apiBase + "/usuariosapi/login", json);
            var usuario = JsonSerializer.Deserialize<UsuarioSesionViewModel>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (usuario == null)
            {
                ModelState.AddModelError("", "Credenciales incorrectas.");
                return View(model);
            }

            HttpContext.Session.SetString("IsLoggedIn",    "true");
            HttpContext.Session.SetString("UsuarioId",     usuario.UsuarioId.ToString());
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            HttpContext.Session.SetString("UsuarioEmail",  usuario.Email);
            HttpContext.Session.SetString("RolNombre",     usuario.RolNombre);

            return RedirectToAction("Index", "Home");
        }
        catch
        {
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View(model);
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
