using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class ReservasController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public ReservasController(IRestProvider restProvider, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/reservasapi");
            var raw = JsonSerializer.Deserialize<List<JsonElement>>(response, JsonOpts) ?? new();

            var reservas = raw.Select(e => new ReservaViewModel
            {
                EventoId = e.GetProperty("eventoId").GetInt32(),
                TipoEvento = e.GetProperty("tipoEvento").GetString() ?? "",
                FechaEvento = e.GetProperty("fechaEvento").GetDateTime(),
                NumPersonas = e.GetProperty("numPersonas").GetInt32(),
                Estado = e.GetProperty("estado").GetString() ?? "",
                EstadoPago = e.GetProperty("estadoPago").GetString() ?? "",
                MontoTotal = e.GetProperty("montoTotal").GetDecimal(),
                MontoPagado = e.GetProperty("montoPagado").GetDecimal(),
                SaldoPendiente = e.TryGetProperty("saldoPendiente", out var sp) && sp.ValueKind != JsonValueKind.Null
                    ? sp.GetDecimal() : 0,
                ClienteNombre = e.TryGetProperty("cliente", out var c) && c.ValueKind != JsonValueKind.Null
                    ? c.GetProperty("nombre").GetString() ?? "" : "",
                PaqueteNombre = e.TryGetProperty("paquete", out var p) && p.ValueKind != JsonValueKind.Null
                    ? p.GetProperty("nombre").GetString() ?? "" : "",
            }).ToList();

            return View(reservas);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar reservas: " + ex.Message;
            return View(new List<ReservaViewModel>());
        }
    }

    public async Task<IActionResult> Crear()
    {
        var model = new ReservaFormViewModel();
        await CargarListas(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ReservaFormViewModel model)
    {
        LimpiarErroresServicios();
        if (!ModelState.IsValid)
        {
            await CargarListas(model);
            return View(model);
        }

        try
        {
            var usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId") ?? "0");

            var json = JsonSerializer.Serialize(new
            {
                model.ClienteId,
                model.PaqueteId,
                model.TipoEvento,
                FechaEvento = model.FechaEvento.ToString("yyyy-MM-dd"),
                model.NumPersonas,
                model.MontoTotal,
                model.Notas,
                UsuarioCreacion = usuarioId,
                Servicios = model.ServiciosSeleccionados
                    .Where(s => s.ServicioId > 0)
                    .Select(s => new { s.ServicioId, s.Cantidad, s.PrecioAcordado })
            });

            await _restProvider.PostAsync(_apiBase + "/reservasapi", json);
            TempData["Exito"] = "Reserva creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al crear la reserva: " + ex.Message);
            await CargarListas(model);
            return View(model);
        }
    }

    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/reservasapi/" + id);
            var e = JsonSerializer.Deserialize<JsonElement>(response, JsonOpts);

            var model = new ReservaFormViewModel
            {
                EventoId = e.GetProperty("eventoId").GetInt32(),
                ClienteId = e.GetProperty("clienteId").GetInt32(),
                PaqueteId = e.GetProperty("paqueteId").GetInt32(),
                TipoEvento = e.GetProperty("tipoEvento").GetString() ?? "",
                FechaEvento = e.GetProperty("fechaEvento").GetDateTime(),
                NumPersonas = e.GetProperty("numPersonas").GetInt32(),
                Estado = e.GetProperty("estado").GetString() ?? "Pendiente",
                MontoTotal = e.GetProperty("montoTotal").GetDecimal(),
                Notas = e.TryGetProperty("notas", out var n) && n.ValueKind != JsonValueKind.Null
                    ? n.GetString() : null,
            };

            if (e.TryGetProperty("eventoServicios", out var servicios))
            {
                foreach (var s in servicios.EnumerateArray())
                {
                    model.ServiciosSeleccionados.Add(new ServicioSeleccionadoViewModel
                    {
                        ServicioId = s.GetProperty("servicioId").GetInt32(),
                        Cantidad = s.GetProperty("cantidad").GetInt32(),
                        PrecioAcordado = s.GetProperty("precioAcordado").GetDecimal()
                    });
                }
            }

            await CargarListas(model);
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al cargar la reserva: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ReservaFormViewModel model)
    {
        LimpiarErroresServicios();
        if (!ModelState.IsValid)
        {
            await CargarListas(model);
            return View(model);
        }

        try
        {
            var json = JsonSerializer.Serialize(new
            {
                model.ClienteId,
                model.PaqueteId,
                model.TipoEvento,
                FechaEvento = model.FechaEvento.ToString("yyyy-MM-dd"),
                model.NumPersonas,
                model.Estado,
                model.MontoTotal,
                model.Notas,
                Servicios = model.ServiciosSeleccionados
                    .Where(s => s.ServicioId > 0)
                    .Select(s => new { s.ServicioId, s.Cantidad, s.PrecioAcordado })
            });

            await _restProvider.PutAsync(_apiBase + "/reservasapi/" + id, json);
            TempData["Exito"] = "Reserva actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al actualizar la reserva: " + ex.Message);
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
            await _restProvider.DeleteAsync(_apiBase + "/reservasapi/" + id);
            TempData["Exito"] = "Reserva eliminada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al eliminar la reserva: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
public IActionResult Calendario() => View();

    [HttpGet]
    public async Task<IActionResult> CalendarioData()
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/reservasapi/calendario");
            return Content(response, "application/json");
        }
        catch
        {
            return Ok("[]");
        }
    }
    private async Task CargarListas(ReservaFormViewModel model)
    {
        try
        {
            var cliResp = await _restProvider.GetAsync(_apiBase + "/clientesapi");
            model.Clientes = JsonSerializer.Deserialize<List<JsonElement>>(cliResp, JsonOpts)?
                .Select(c => new ClienteOpcionViewModel
                {
                    ClienteId = c.GetProperty("clienteId").GetInt32(),
                    Nombre = c.GetProperty("nombre").GetString() ?? "",
                    Documento = c.GetProperty("documento").GetString() ?? ""
                }).ToList() ?? new();
        }
        catch { model.Clientes = new(); }

        try
        {
            var pakResp = await _restProvider.GetAsync(_apiBase + "/paquetesapi/activos");
            model.Paquetes = JsonSerializer.Deserialize<List<JsonElement>>(pakResp, JsonOpts)?
                .Select(p => new PaqueteOpcionViewModel
                {
                    PaqueteId = p.GetProperty("paqueteId").GetInt32(),
                    Nombre = p.GetProperty("nombre").GetString() ?? "",
                    PrecioBase = p.GetProperty("precioBase").GetDecimal(),
                    MaxPersonas = p.GetProperty("maxPersonas").GetInt32()
                }).ToList() ?? new();
        }
        catch { model.Paquetes = new(); }

        try
        {
            var srvResp = await _restProvider.GetAsync(_apiBase + "/serviciosapi/activos");
            model.ServiciosDisponibles = JsonSerializer.Deserialize<List<JsonElement>>(srvResp, JsonOpts)?
                .Select(s => new ServicioOpcionViewModel
                {
                    ServicioId = s.GetProperty("servicioId").GetInt32(),
                    Nombre = s.GetProperty("nombre").GetString() ?? "",
                    Categoria = s.GetProperty("categoria").GetString() ?? "",
                    PrecioUnitario = s.GetProperty("precioUnitario").GetDecimal()
                }).ToList() ?? new();
        }
        catch { model.ServiciosDisponibles = new(); }
    }

    private void LimpiarErroresServicios()
    {
        var keys = ModelState.Keys
            .Where(k => k.StartsWith("ServiciosSeleccionados"))
            .ToList();
        foreach (var k in keys)
            ModelState.Remove(k);
    }
}
