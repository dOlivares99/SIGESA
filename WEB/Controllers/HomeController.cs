using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WEB.Models;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class HomeController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public HomeController(IRestProvider restProvider, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _restProvider.GetAsync(_apiBase + "/dashboardapi");
            var data = JsonSerializer.Deserialize<JsonElement>(response, JsonOpts);

            var model = new DashboardViewModel
            {
                EventosMes = data.GetProperty("eventosMes").GetInt32(),
                IngresosMes = data.GetProperty("ingresosMes").GetDecimal(),
                SaldoPendienteTotal = data.GetProperty("saldoPendienteTotal").GetDecimal(),
                TotalClientes = data.GetProperty("totalClientes").GetInt32(),
                TieneSaldosPendientes = data.GetProperty("tieneSaldosPendientes").GetBoolean(),
                MesActual = data.GetProperty("mesActual").GetString() ?? "",
            };

            foreach (var e in data.GetProperty("proximosEventos").EnumerateArray())
                model.ProximosEventos.Add(new ProximoEventoViewModel
                {
                    EventoId = e.GetProperty("eventoId").GetInt32(),
                    TipoEvento = e.GetProperty("tipoEvento").GetString() ?? "",
                    FechaEvento = e.GetProperty("fechaEvento").GetString() ?? "",
                    Estado = e.GetProperty("estado").GetString() ?? "",
                    NumPersonas = e.GetProperty("numPersonas").GetInt32(),
                    MontoTotal = e.GetProperty("montoTotal").GetDecimal(),
                    SaldoPendiente = e.GetProperty("saldoPendiente").GetDecimal(),
                    ClienteNombre = e.GetProperty("clienteNombre").GetString() ?? "",
                    PaqueteNombre = e.GetProperty("paqueteNombre").GetString() ?? "",
                });

            foreach (var a in data.GetProperty("alertasSaldo").EnumerateArray())
                model.AlertasSaldo.Add(new AlertaSaldoViewModel
                {
                    EventoId = a.GetProperty("eventoId").GetInt32(),
                    TipoEvento = a.GetProperty("tipoEvento").GetString() ?? "",
                    FechaEvento = a.GetProperty("fechaEvento").GetString() ?? "",
                    SaldoPendiente = a.GetProperty("saldoPendiente").GetDecimal(),
                    MontoTotal = a.GetProperty("montoTotal").GetDecimal(),
                    ClienteNombre = a.GetProperty("clienteNombre").GetString() ?? "",
                });

            foreach (var m in data.GetProperty("graficaMeses").EnumerateArray())
                model.GraficaMeses.Add(new GraficaMesViewModel
                {
                    Mes = m.GetProperty("mes").GetString() ?? "",
                    Cantidad = m.GetProperty("cantidad").GetInt32()
                });

            foreach (var p in data.GetProperty("ultimosPagos").EnumerateArray())
                model.UltimosPagos.Add(new UltimoPagoViewModel
                {
                    PagoId = p.GetProperty("pagoId").GetInt32(),
                    Monto = p.GetProperty("monto").GetDecimal(),
                    FechaPago = p.GetProperty("fechaPago").GetString() ?? "",
                    TipoPago = p.GetProperty("tipoPago").GetString() ?? "",
                    ClienteNombre = p.GetProperty("clienteNombre").GetString() ?? "",
                    TipoEvento = p.GetProperty("tipoEvento").GetString() ?? "",
                });

            foreach (var e in data.GetProperty("porEstado").EnumerateArray())
                model.PorEstado.Add(new EstadoEventoViewModel
                {
                    Estado = e.GetProperty("estado").GetString() ?? "",
                    Cantidad = e.GetProperty("cantidad").GetInt32()
                });

            return View(model);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar el dashboard: " + ex.Message;
            return View(new DashboardViewModel());
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}