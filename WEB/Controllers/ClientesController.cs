using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class ClientesController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    public ClientesController(IRestProvider restProvider, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    // GET /Clientes
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/clientesapi");
            var clientes = JsonSerializer.Deserialize<List<ClienteViewModel>>(response, JsonOpts)
                           ?? new List<ClienteViewModel>();
            return View(clientes);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar clientes: " + ex.Message;
            return View(new List<ClienteViewModel>());
        }
    }

    // GET /Clientes/Crear
    public IActionResult Crear() => View(new ClienteFormViewModel());

    // POST /Clientes/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ClienteFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Nombre,
                model.Documento,
                model.Telefono,
                model.Email
            });
            await _restProvider.PostAsync(_apiBase + "/clientesapi", json);
            TempData["Exito"] = "Cliente creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al crear cliente: " + ex.Message);
            return View(model);
        }
    }

    // GET /Clientes/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/clientesapi/" + id);
            var cliente = JsonSerializer.Deserialize<ClienteViewModel>(response, JsonOpts);
            if (cliente == null) return NotFound();

            var model = new ClienteFormViewModel
            {
                ClienteId = cliente.ClienteId,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Telefono = cliente.Telefono,
                Email = cliente.Email
            };
            return View(model);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar cliente: " + ex.Message;
            return NotFound();
        }
    }

    // POST /Clientes/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ClienteFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.Nombre,
                model.Documento,
                model.Telefono,
                model.Email
            });
            await _restProvider.PutAsync(_apiBase + "/clientesapi/" + id, json);
            TempData["Exito"] = "Cliente actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al actualizar cliente: " + ex.Message);
            return View(model);
        }
    }

    // GET /Clientes/Historial/5
    public async Task<IActionResult> Historial(int id)
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/clientesapi/" + id + "/historial");
            var data = JsonSerializer.Deserialize<JsonElement>(response, JsonOpts);

            var model = new ClienteHistorialViewModel
            {
                ClienteId = data.GetProperty("clienteId").GetInt32(),
                Nombre = data.GetProperty("nombre").GetString() ?? "",
                Documento = data.GetProperty("documento").GetString() ?? "",
                Telefono = data.GetProperty("telefono").GetString() ?? "",
                Email = data.TryGetProperty("email", out var em) ? em.GetString() : null,
            };

            if (data.TryGetProperty("eventos", out var eventos))
            {
                foreach (var e in eventos.EnumerateArray())
                {
                    var paquete = e.TryGetProperty("paquete", out var p) && p.ValueKind != JsonValueKind.Null
                        ? p.GetProperty("nombre").GetString() ?? ""
                        : "—";

                    model.Eventos.Add(new EventoResumenViewModel
                    {
                        EventoId = e.GetProperty("eventoId").GetInt32(),
                        TipoEvento = e.GetProperty("tipoEvento").GetString() ?? "",
                        FechaEvento = e.GetProperty("fechaEvento").GetDateTime(),
                        Estado = e.GetProperty("estado").GetString() ?? "",
                        EstadoPago = e.GetProperty("estadoPago").GetString() ?? "",
                        MontoTotal = e.GetProperty("montoTotal").GetDecimal(),
                        MontoPagado = e.GetProperty("montoPagado").GetDecimal(),
                        SaldoPendiente = e.GetProperty("saldoPendiente").GetDecimal(),
                        PaqueteNombre = paquete
                    });
                }
            }

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al cargar historial: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}