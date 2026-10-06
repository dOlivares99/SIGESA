using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WEB.Models.ViewModels;
using WEB.Services;

namespace WEB.Controllers;

public class ReportesController : Controller
{
    private readonly IRestProvider _restProvider;
    private readonly string _apiBase;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public ReportesController(IRestProvider restProvider, IConfiguration configuration)
    {
        _restProvider = restProvider;
        _apiBase = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5119/api";
    }

    // ================================================================
    // HU-REP-01 — Ingresos por periodo
    // ================================================================

    public IActionResult Index() => View(new ReporteViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generar(ReporteViewModel filtro)
    {
        try
        {
            var url = $"{_apiBase}/reportesapi?desde={filtro.Desde:yyyy-MM-dd}&hasta={filtro.Hasta:yyyy-MM-dd}";
            var response = await _restProvider.GetAsync(url);
            var data = JsonSerializer.Deserialize<JsonElement>(response, JsonOpts);

            var resultado = new ReporteResultadoViewModel
            {
                SinDatos = data.GetProperty("sinDatos").GetBoolean(),
                Desde = filtro.Desde.ToString("dd/MM/yyyy"),
                Hasta = filtro.Hasta.ToString("dd/MM/yyyy"),
            };

            if (resultado.SinDatos)
            {
                resultado.Mensaje = data.GetProperty("mensaje").GetString() ?? "";
                ViewBag.Filtro = filtro;
                return View("Resultado", resultado);
            }

            resultado.TotalIngresos = data.GetProperty("totalIngresos").GetDecimal();
            resultado.TotalPagos = data.GetProperty("totalPagos").GetInt32();

            foreach (var d in data.GetProperty("desglose").EnumerateArray())
            {
                resultado.Desglose.Add(new DesgloseViewModel
                {
                    TipoEvento = d.GetProperty("tipoEvento").GetString() ?? "",
                    Cantidad = d.GetProperty("cantidad").GetInt32(),
                    Total = d.GetProperty("total").GetDecimal()
                });
            }

            foreach (var d in data.GetProperty("detalle").EnumerateArray())
            {
                resultado.Detalle.Add(new DetallepagoViewModel
                {
                    PagoId = d.GetProperty("pagoId").GetInt32(),
                    FechaPago = d.GetProperty("fechaPago").GetString() ?? "",
                    ClienteNombre = d.GetProperty("clienteNombre").GetString() ?? "",
                    TipoEvento = d.GetProperty("tipoEvento").GetString() ?? "",
                    TipoPago = d.GetProperty("tipoPago").GetString() ?? "",
                    Monto = d.GetProperty("monto").GetDecimal()
                });
            }

            ViewBag.Filtro = filtro;
            return View("Resultado", resultado);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al generar el reporte. Intente más tarde.";
            return View("Index", filtro);
        }
    }

    public IActionResult DescargarExcel(DateTime desde, DateTime hasta)
    {
        var url = $"{_apiBase}/reportesapi/excel?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        return Redirect(url);
    }

    public IActionResult DescargarPdf(DateTime desde, DateTime hasta)
    {
        var url = $"{_apiBase}/reportesapi/pdf?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        return Redirect(url);
    }

    // ================================================================
    // HU-REP-02 — Ocupación mensual
    // ================================================================

    public async Task<IActionResult> Ocupacion(int? anio)
    {
        var anioConsulta = anio ?? DateTime.Now.Year;
        try
        {
            var url = $"{_apiBase}/reportesapi/ocupacion?anio={anioConsulta}";
            var response = await _restProvider.GetAsync(url);
            var data = JsonSerializer.Deserialize<JsonElement>(response, JsonOpts);

            var model = new OcupacionViewModel
            {
                Anio = anioConsulta,
                TotalEventos = data.GetProperty("totalEventos").GetInt32(),
                MesMaximo = data.GetProperty("mesMaximo").GetString() ?? "",
                CantidadMesMaximo = data.GetProperty("cantidadMesMaximo").GetInt32(),
            };

            foreach (var m in data.GetProperty("porMes").EnumerateArray())
            {
                model.PorMes.Add(new MesOcupacionViewModel
                {
                    Mes = m.GetProperty("mes").GetInt32(),
                    Nombre = m.GetProperty("nombre").GetString() ?? "",
                    Cantidad = m.GetProperty("cantidad").GetInt32()
                });
            }

            foreach (var s in data.GetProperty("servicios").EnumerateArray())
            {
                model.Servicios.Add(new ServicioOcupacionViewModel
                {
                    Nombre = s.GetProperty("nombre").GetString() ?? "",
                    Categoria = s.GetProperty("categoria").GetString() ?? "",
                    Frecuencia = s.GetProperty("frecuencia").GetInt32(),
                    Ingresos = s.GetProperty("ingresos").GetDecimal()
                });
            }

            return View(model);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al cargar el reporte. Intente más tarde.";
            return View(new OcupacionViewModel { Anio = anioConsulta });
        }
    }

    public IActionResult OcupacionExcel(int? anio)
    {
        var url = $"{_apiBase}/reportesapi/ocupacion/excel?anio={anio ?? DateTime.Now.Year}";
        return Redirect(url);
    }

    public IActionResult OcupacionPdf(int? anio)
    {
        var url = $"{_apiBase}/reportesapi/ocupacion/pdf?anio={anio ?? DateTime.Now.Year}";
        return Redirect(url);
    }
}