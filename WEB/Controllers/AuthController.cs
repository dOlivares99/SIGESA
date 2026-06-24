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
            var json = JsonSerializer.Serialize(new { model.Email, model.Password });
            var response = await _restProvider.PostAsync(_apiBase + "/usuariosapi/login", json);
            var usuario = JsonSerializer.Deserialize<UsuarioSesionViewModel>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (usuario == null)
            {
                ModelState.AddModelError("", "Credenciales incorrectas.");
                return View(model);
            }

            HttpContext.Session.SetString("IsLoggedIn", "true");
            HttpContext.Session.SetString("UsuarioId", usuario.UsuarioId.ToString());
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
            HttpContext.Session.SetString("RolNombre", usuario.RolNombre);

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

    // ── Recuperar contraseña — paso 1 ────────────────────────────────
    public IActionResult RecuperarContrasena() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecuperarContrasena(RecuperarContrasenaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var json = JsonSerializer.Serialize(new { model.Email });
            await _restProvider.PostAsync(_apiBase + "/recuperacionapi/solicitar", json);
        }
        catch { }

        TempData["RecuperacionEnviada"] = model.Email;
        return RedirectToAction(nameof(RecuperacionEnviada));
    }

    public IActionResult RecuperacionEnviada()
    {
        ViewBag.Email = TempData["RecuperacionEnviada"] as string ?? "";
        return View();
    }

    // ── Recuperar contraseña — paso 2 ────────────────────────────────
    public async Task<IActionResult> NuevaContrasena(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction(nameof(Login));

        try
        {
            var resp = await _restProvider.GetAsync(
                _apiBase + "/recuperacionapi/validar?token=" + Uri.EscapeDataString(token));
            var json = JsonSerializer.Deserialize<JsonElement>(resp,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!json.GetProperty("valido").GetBoolean())
            {
                TempData["Error"] = "El enlace de recuperación no es válido o ya expiró.";
                return RedirectToAction(nameof(Login));
            }
        }
        catch
        {
            TempData["Error"] = "No se pudo validar el enlace.";
            return RedirectToAction(nameof(Login));
        }

        return View(new NuevaContrasenaViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevaContrasena(NuevaContrasenaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Token,
                NuevaContrasena = model.Contrasena
            });
            await _restProvider.PostAsync(_apiBase + "/recuperacionapi/restablecer", json);
            TempData["Exito"] = "Contraseña actualizada. Inicia sesión con tu nueva contraseña.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "No se pudo cambiar la contraseña: " + ex.Message);
            return View(model);
        }
    }
}
