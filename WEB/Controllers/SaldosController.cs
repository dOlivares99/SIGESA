using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class SaldosController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public SaldosController(IRestProvider restProvider, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    // GET /Saldos
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/reservasapi");
            var raw = JsonSerializer.Deserialize<List<JsonElement>>(response, JsonOpts) ?? new();

            var saldos = raw.Select(e => new SaldoViewModel
            {
                EventoId = e.GetProperty("eventoId").GetInt32(),
                TipoEvento = e.GetProperty("tipoEvento").GetString() ?? "",
                FechaEvento = e.GetProperty("fechaEvento").GetDateTime(),
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
            })
            .OrderByDescending(s => s.SaldoPendiente)
            .ToList();

            // Totales para el resumen
            ViewBag.TotalMonto = saldos.Sum(s => s.MontoTotal);
            ViewBag.TotalPagado = saldos.Sum(s => s.MontoPagado);
            ViewBag.TotalSaldo = saldos.Sum(s => s.SaldoPendiente);
            ViewBag.CountPendiente = saldos.Count(s => s.EstadoPago != "Saldado");

            return View(saldos);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar saldos: " + ex.Message;
            return View(new List<SaldoViewModel>());
        }
    }
}