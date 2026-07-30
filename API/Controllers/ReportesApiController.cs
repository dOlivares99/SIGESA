using ClosedXML.Excel;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportesApiController(SIGESAContext context) : ControllerBase
{
    private const string MoradoPrincipal = "#534AB7";
    private const string MoradoClaro = "#F0EEFF";
    private const string FondoSeccion = "#F8F7FF";
    private const string BordeClaro = "#ECE9FF";
    private const string Verde = "#0F6E56";
    private const string VerdeClaro = "#E6F4F0";
    private const string TextoOscuro = "#1E1E1E";
    private const string TextoGris = "#6C757D";

    // ================================================================
    // HU-REP-01 — Reporte de ingresos por periodo
    // ================================================================

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime desde,
                                         [FromQuery] DateTime hasta)
    {
        var desdeDate = DateOnly.FromDateTime(desde);
        var hastaDate = DateOnly.FromDateTime(hasta);

        var pagos = await context.Pagos
            .Include(p => p.Evento).ThenInclude(e => e.Cliente)
            .Include(p => p.TipoPago)
            .Where(p => DateOnly.FromDateTime(p.FechaPago) >= desdeDate &&
                        DateOnly.FromDateTime(p.FechaPago) <= hastaDate)
            .OrderBy(p => p.FechaPago)
            .ToListAsync();

        if (!pagos.Any())
            return Ok(new
            {
                sinDatos = true,
                mensaje = "Sin información disponible para el periodo seleccionado."
            });

        var totalIngresos = pagos.Sum(p => p.Monto);

        var desglose = pagos
            .GroupBy(p => p.Evento.TipoEvento)
            .Select(g => new
            {
                tipoEvento = g.Key,
                cantidad = g.Count(),
                total = g.Sum(p => p.Monto)
            })
            .OrderByDescending(g => g.total)
            .ToList();

        var detalle = pagos.Select(p => new
        {
            pagoId = p.PagoId,
            fechaPago = p.FechaPago.ToString("dd/MM/yyyy"),
            clienteNombre = p.Evento.Cliente.Nombre,
            tipoEvento = p.Evento.TipoEvento,
            tipoPago = p.TipoPago.Nombre,
            monto = p.Monto
        }).ToList();

        return Ok(new
        {
            sinDatos = false,
            desde = desdeDate.ToString("dd/MM/yyyy"),
            hasta = hastaDate.ToString("dd/MM/yyyy"),
            totalIngresos,
            totalPagos = pagos.Count,
            desglose,
            detalle
        });
    }

    [HttpGet("excel")]
    public async Task<IActionResult> Excel([FromQuery] DateTime desde,
                                            [FromQuery] DateTime hasta)
    {
        var desdeDate = DateOnly.FromDateTime(desde);
        var hastaDate = DateOnly.FromDateTime(hasta);

        var pagos = await context.Pagos
            .Include(p => p.Evento).ThenInclude(e => e.Cliente)
            .Include(p => p.TipoPago)
            .Where(p => DateOnly.FromDateTime(p.FechaPago) >= desdeDate &&
                        DateOnly.FromDateTime(p.FechaPago) <= hastaDate)
            .OrderBy(p => p.FechaPago)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Ingresos");

        ws.Cell(1, 1).Value = "SIGESA — Sala de Eventos Mónica";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(MoradoPrincipal);
        ws.Range(1, 1, 1, 6).Merge();

        ws.Cell(2, 1).Value = "Reporte de Ingresos por Periodo";
        ws.Cell(2, 1).Style.Font.FontSize = 11;
        ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml(TextoGris);
        ws.Range(2, 1, 2, 6).Merge();

        ws.Cell(3, 1).Value = $"Periodo: {desdeDate:dd/MM/yyyy} — {hastaDate:dd/MM/yyyy}";
        ws.Cell(3, 1).Style.Font.FontSize = 10;
        ws.Cell(3, 1).Style.Font.FontColor = XLColor.FromHtml(TextoGris);
        ws.Range(3, 1, 3, 6).Merge();

        ws.Row(4).Height = 8;

        var headers = new[] { "#", "Fecha", "Cliente", "Tipo de evento", "Tipo de pago", "Monto (₡)" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(5, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml(MoradoPrincipal);
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml(BordeClaro);
        }

        int row = 6;
        bool alt = false;
        foreach (var p in pagos)
        {
            var bg = alt ? XLColor.FromHtml(FondoSeccion) : XLColor.White;
            alt = !alt;
            ws.Cell(row, 1).Value = p.PagoId;
            ws.Cell(row, 2).Value = p.FechaPago.ToString("dd/MM/yyyy");
            ws.Cell(row, 3).Value = p.Evento.Cliente.Nombre;
            ws.Cell(row, 4).Value = p.Evento.TipoEvento;
            ws.Cell(row, 5).Value = p.TipoPago.Nombre;
            ws.Cell(row, 6).Value = (double)p.Monto;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            for (int col = 1; col <= 6; col++)
            {
                ws.Cell(row, col).Style.Fill.BackgroundColor = bg;
                ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(row, col).Style.Border.OutsideBorderColor = XLColor.FromHtml(BordeClaro);
            }
            row++;
        }

        ws.Cell(row, 5).Value = "TOTAL";
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.Font.FontColor = XLColor.FromHtml(MoradoPrincipal);
        ws.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml(MoradoClaro);
        ws.Cell(row, 6).Value = (double)pagos.Sum(p => p.Monto);
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.Font.FontColor = XLColor.FromHtml(MoradoPrincipal);
        ws.Cell(row, 6).Style.Fill.BackgroundColor = XLColor.FromHtml(MoradoClaro);
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"reporte-ingresos-{desdeDate:yyyyMMdd}-{hastaDate:yyyyMMdd}.xlsx");
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> Pdf([FromQuery] DateTime desde,
                                          [FromQuery] DateTime hasta)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var desdeDate = DateOnly.FromDateTime(desde);
        var hastaDate = DateOnly.FromDateTime(hasta);

        var pagos = await context.Pagos
            .Include(p => p.Evento).ThenInclude(e => e.Cliente)
            .Include(p => p.TipoPago)
            .Where(p => DateOnly.FromDateTime(p.FechaPago) >= desdeDate &&
                        DateOnly.FromDateTime(p.FechaPago) <= hastaDate)
            .OrderBy(p => p.FechaPago)
            .ToListAsync();

        var totalIngresos = pagos.Sum(p => p.Monto);
        var desgloseGrupos = pagos
            .GroupBy(p => p.Evento.TipoEvento)
            .Select(g => new { TipoEvento = g.Key, Total = g.Sum(p => p.Monto), Cantidad = g.Count() })
            .OrderByDescending(g => g.Total)
            .ToList();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(s => s.FontSize(10).FontColor(TextoOscuro));

                page.Header()
                    .Background(MoradoPrincipal)
                    .PaddingVertical(22).PaddingHorizontal(35)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SIGESA").FontSize(25).Bold().FontColor(Colors.White);
                            col.Item().Text("Sala de Eventos Mónica").FontSize(10).FontColor("#DDD9FF");
                        });
                        row.ConstantItem(220).AlignRight().AlignMiddle().Column(col =>
                        {
                            col.Item().AlignRight().Text("REPORTE DE INGRESOS")
                                .FontSize(16).Bold().FontColor(Colors.White);
                            col.Item().AlignRight()
                                .Text($"Periodo: {desdeDate:dd/MM/yyyy} — {hastaDate:dd/MM/yyyy}")
                                .FontSize(9).FontColor("#DDD9FF");
                        });
                    });

                page.Content().PaddingHorizontal(35).PaddingVertical(25).Column(col =>
                {
                    col.Spacing(16);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Background(MoradoClaro).Border(1).BorderColor(BordeClaro)
                            .Padding(14).Column(c =>
                            {
                                c.Item().Text("Total ingresos").FontSize(8).FontColor(TextoGris);
                                c.Item().Text($"₡{totalIngresos:N2}").FontSize(16).Bold().FontColor(MoradoPrincipal);
                            });
                        row.ConstantItem(16);
                        row.RelativeItem().Background(VerdeClaro).Border(1).BorderColor("#B2DDD5")
                            .Padding(14).Column(c =>
                            {
                                c.Item().Text("Total pagos registrados").FontSize(8).FontColor(TextoGris);
                                c.Item().Text($"{pagos.Count}").FontSize(16).Bold().FontColor(Verde);
                            });
                    });

                    col.Item().Background(FondoSeccion).Border(1).BorderColor(BordeClaro)
                        .Padding(15).Column(seccion =>
                        {
                            seccion.Spacing(10);
                            seccion.Item().Text("DESGLOSE POR TIPO DE EVENTO")
                                .Bold().FontSize(12).FontColor(MoradoPrincipal);
                            seccion.Item().LineHorizontal(1).LineColor(BordeClaro);
                            seccion.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(4);
                                    cols.RelativeColumn(1);
                                    cols.RelativeColumn(2);
                                });
                                table.Header(header =>
                                {
                                    header.Cell().Element(EstiloEncabezado).Text("Tipo de evento");
                                    header.Cell().Element(EstiloEncabezado).AlignCenter().Text("Cantidad");
                                    header.Cell().Element(EstiloEncabezado).AlignRight().Text("Total");
                                });
                                foreach (var g in desgloseGrupos)
                                {
                                    table.Cell().Element(EstiloCelda).Text(g.TipoEvento);
                                    table.Cell().Element(EstiloCelda).AlignCenter().Text(g.Cantidad.ToString());
                                    table.Cell().Element(EstiloCelda).AlignRight().Text($"₡{g.Total:N2}");
                                }
                            });
                        });

                    col.Item().Background(FondoSeccion).Border(1).BorderColor(BordeClaro)
                        .Padding(15).Column(seccion =>
                        {
                            seccion.Spacing(10);
                            seccion.Item().Text("DETALLE DE PAGOS")
                                .Bold().FontSize(12).FontColor(MoradoPrincipal);
                            seccion.Item().LineHorizontal(1).LineColor(BordeClaro);
                            seccion.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(28);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(3);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(2);
                                });
                                table.Header(header =>
                                {
                                    header.Cell().Element(EstiloEncabezado).Text("#");
                                    header.Cell().Element(EstiloEncabezado).Text("Fecha");
                                    header.Cell().Element(EstiloEncabezado).Text("Cliente");
                                    header.Cell().Element(EstiloEncabezado).Text("Tipo evento");
                                    header.Cell().Element(EstiloEncabezado).Text("Tipo pago");
                                    header.Cell().Element(EstiloEncabezado).AlignRight().Text("Monto");
                                });
                                foreach (var p in pagos)
                                {
                                    table.Cell().Element(EstiloCelda).Text(p.PagoId.ToString());
                                    table.Cell().Element(EstiloCelda).Text(p.FechaPago.ToString("dd/MM/yyyy"));
                                    table.Cell().Element(EstiloCelda).Text(p.Evento.Cliente.Nombre);
                                    table.Cell().Element(EstiloCelda).Text(p.Evento.TipoEvento);
                                    table.Cell().Element(EstiloCelda).Text(p.TipoPago.Nombre);
                                    table.Cell().Element(EstiloCelda).AlignRight().Text($"₡{p.Monto:N2}");
                                }
                            });
                        });

                    col.Item().AlignRight().Width(270)
                        .Background(MoradoClaro).Border(1).BorderColor(BordeClaro)
                        .Padding(16).Column(totalCol =>
                        {
                            totalCol.Spacing(7);
                            totalCol.Item().PaddingVertical(4).LineHorizontal(1).LineColor(MoradoPrincipal);
                            totalCol.Item().Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL INGRESOS")
                                    .FontSize(13).Bold().FontColor(MoradoPrincipal);
                                row.ConstantItem(130).AlignRight().Text($"₡{totalIngresos:N2}")
                                    .FontSize(13).Bold().FontColor(MoradoPrincipal);
                            });
                        });
                });

                page.Footer().PaddingHorizontal(35).PaddingVertical(15)
                    .BorderTop(1).BorderColor(BordeClaro)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SIGESA · Sala de Eventos Mónica")
                                .Bold().FontSize(9).FontColor(MoradoPrincipal);
                            col.Item().Text("Reporte generado automáticamente.")
                                .FontSize(8).FontColor(TextoGris);
                        });
                        row.RelativeItem().AlignRight().AlignMiddle().Text(text =>
                        {
                            text.Span("Página ").FontSize(8).FontColor(TextoGris);
                            text.CurrentPageNumber().FontSize(8).FontColor(TextoGris);
                            text.Span(" de ").FontSize(8).FontColor(TextoGris);
                            text.TotalPages().FontSize(8).FontColor(TextoGris);
                        });
                    });
            });
        });

        return File(pdf.GeneratePdf(), "application/pdf",
            $"reporte-ingresos-{desdeDate:yyyyMMdd}-{hastaDate:yyyyMMdd}.pdf");
    }

    // ================================================================
    // HU-REP-02 — Reporte de ocupación mensual
    // ================================================================

    [HttpGet("ocupacion")]
    public async Task<IActionResult> Ocupacion([FromQuery] int? anio)
    {
        var anioConsulta = anio ?? DateTime.Now.Year;

        var eventos = await context.Eventos
            .Where(e => e.FechaEvento.Year == anioConsulta && e.Estado != "Cancelada")
            .ToListAsync();

        var porMes = Enumerable.Range(1, 12).Select(mes => new
        {
            mes = mes,
            nombre = new DateTime(anioConsulta, mes, 1).ToString("MMM"),
            cantidad = eventos.Count(e => e.FechaEvento.Month == mes)
        }).ToList();

        var servicios = await context.EventoServicios
            .Include(es => es.Servicio)
            .Include(es => es.Evento)
            .Where(es => es.Evento.FechaEvento.Year == anioConsulta &&
                         es.Evento.Estado != "Cancelada")
            .GroupBy(es => new { es.ServicioId, es.Servicio.Nombre, es.Servicio.Categoria })
            .Select(g => new
            {
                servicioId = g.Key.ServicioId,
                nombre = g.Key.Nombre,
                categoria = g.Key.Categoria,
                frecuencia = g.Sum(es => es.Cantidad),
                ingresos = g.Sum(es => es.Cantidad * es.PrecioAcordado)
            })
            .OrderByDescending(s => s.frecuencia)
            .ToListAsync();

        var mesMaximo = porMes.OrderByDescending(m => m.cantidad).First();

        return Ok(new
        {
            anio = anioConsulta,
            totalEventos = eventos.Count,
            mesMaximo = mesMaximo.nombre,
            cantidadMesMaximo = mesMaximo.cantidad,
            porMes,
            servicios
        });
    }

    [HttpGet("ocupacion/excel")]
    public async Task<IActionResult> OcupacionExcel([FromQuery] int? anio)
    {
        var anioConsulta = anio ?? DateTime.Now.Year;

        var eventos = await context.Eventos
            .Where(e => e.FechaEvento.Year == anioConsulta && e.Estado != "Cancelada")
            .ToListAsync();

        var servicios = await context.EventoServicios
            .Include(es => es.Servicio)
            .Include(es => es.Evento)
            .Where(es => es.Evento.FechaEvento.Year == anioConsulta &&
                         es.Evento.Estado != "Cancelada")
            .GroupBy(es => new { es.ServicioId, es.Servicio.Nombre, es.Servicio.Categoria })
            .Select(g => new
            {
                nombre = g.Key.Nombre,
                categoria = g.Key.Categoria,
                frecuencia = g.Sum(es => es.Cantidad),
                ingresos = g.Sum(es => es.Cantidad * es.PrecioAcordado)
            })
            .OrderByDescending(s => s.frecuencia)
            .ToListAsync();

        using var wb = new XLWorkbook();

        // Hoja 1 — Eventos por mes
        var ws1 = wb.Worksheets.Add("Ocupacion por mes");
        ws1.Cell(1, 1).Value = "SIGESA — Ocupación Mensual";
        ws1.Cell(1, 1).Style.Font.Bold = true;
        ws1.Cell(1, 1).Style.Font.FontSize = 14;
        ws1.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(MoradoPrincipal);
        ws1.Range(1, 1, 1, 3).Merge();
        ws1.Cell(2, 1).Value = $"Año: {anioConsulta}";
        ws1.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml(TextoGris);
        ws1.Range(2, 1, 2, 3).Merge();
        ws1.Row(3).Height = 8;

        var h1 = new[] { "Mes", "Nombre", "Eventos" };
        for (int i = 0; i < h1.Length; i++)
        {
            ws1.Cell(4, i + 1).Value = h1[i];
            ws1.Cell(4, i + 1).Style.Font.Bold = true;
            ws1.Cell(4, i + 1).Style.Font.FontColor = XLColor.White;
            ws1.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(MoradoPrincipal);
        }

        bool alt = false;
        for (int mes = 1; mes <= 12; mes++)
        {
            var bg = alt ? XLColor.FromHtml(FondoSeccion) : XLColor.White;
            alt = !alt;
            ws1.Cell(mes + 4, 1).Value = mes;
            ws1.Cell(mes + 4, 2).Value = new DateTime(anioConsulta, mes, 1).ToString("MMMM");
            ws1.Cell(mes + 4, 3).Value = eventos.Count(e => e.FechaEvento.Month == mes);
            for (int col = 1; col <= 3; col++)
                ws1.Cell(mes + 4, col).Style.Fill.BackgroundColor = bg;
        }
        ws1.Columns().AdjustToContents();

        // Hoja 2 — Servicios
        var ws2 = wb.Worksheets.Add("Servicios");
        ws2.Cell(1, 1).Value = "SIGESA — Servicios más contratados";
        ws2.Cell(1, 1).Style.Font.Bold = true;
        ws2.Cell(1, 1).Style.Font.FontSize = 14;
        ws2.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(MoradoPrincipal);
        ws2.Range(1, 1, 1, 4).Merge();
        ws2.Row(3).Height = 8;

        var h2 = new[] { "Servicio", "Categoría", "Frecuencia", "Ingresos (₡)" };
        for (int i = 0; i < h2.Length; i++)
        {
            ws2.Cell(4, i + 1).Value = h2[i];
            ws2.Cell(4, i + 1).Style.Font.Bold = true;
            ws2.Cell(4, i + 1).Style.Font.FontColor = XLColor.White;
            ws2.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(MoradoPrincipal);
        }

        alt = false;
        int row = 5;
        foreach (var s in servicios)
        {
            var bg = alt ? XLColor.FromHtml(FondoSeccion) : XLColor.White;
            alt = !alt;
            ws2.Cell(row, 1).Value = s.nombre;
            ws2.Cell(row, 2).Value = s.categoria;
            ws2.Cell(row, 3).Value = s.frecuencia;
            ws2.Cell(row, 4).Value = (double)s.ingresos;
            ws2.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            for (int col = 1; col <= 4; col++)
                ws2.Cell(row, col).Style.Fill.BackgroundColor = bg;
            row++;
        }
        ws2.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"reporte-ocupacion-{anioConsulta}.xlsx");
    }

    [HttpGet("ocupacion/pdf")]
    public async Task<IActionResult> OcupacionPdf([FromQuery] int? anio)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var anioConsulta = anio ?? DateTime.Now.Year;

        var eventos = await context.Eventos
            .Where(e => e.FechaEvento.Year == anioConsulta && e.Estado != "Cancelada")
            .ToListAsync();

        var servicios = await context.EventoServicios
            .Include(es => es.Servicio)
            .Include(es => es.Evento)
            .Where(es => es.Evento.FechaEvento.Year == anioConsulta &&
                         es.Evento.Estado != "Cancelada")
            .GroupBy(es => new { es.ServicioId, es.Servicio.Nombre, es.Servicio.Categoria })
            .Select(g => new
            {
                nombre = g.Key.Nombre,
                categoria = g.Key.Categoria,
                frecuencia = g.Sum(es => es.Cantidad),
                ingresos = g.Sum(es => es.Cantidad * es.PrecioAcordado)
            })
            .OrderByDescending(s => s.frecuencia)
            .ToListAsync();

        var porMes = Enumerable.Range(1, 12).Select(mes => new
        {
            nombre = new DateTime(anioConsulta, mes, 1).ToString("MMM"),
            cantidad = eventos.Count(e => e.FechaEvento.Month == mes)
        }).ToList();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(s => s.FontSize(10).FontColor(TextoOscuro));

                page.Header()
                    .Background(MoradoPrincipal)
                    .PaddingVertical(22).PaddingHorizontal(35)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SIGESA").FontSize(25).Bold().FontColor(Colors.White);
                            col.Item().Text("Sala de Eventos Mónica").FontSize(10).FontColor("#DDD9FF");
                        });
                        row.ConstantItem(220).AlignRight().AlignMiddle().Column(col =>
                        {
                            col.Item().AlignRight().Text("REPORTE DE OCUPACIÓN")
                                .FontSize(16).Bold().FontColor(Colors.White);
                            col.Item().AlignRight().Text($"Año {anioConsulta}")
                                .FontSize(10).FontColor("#DDD9FF");
                        });
                    });

                page.Content().PaddingHorizontal(35).PaddingVertical(25).Column(col =>
                {
                    col.Spacing(16);

                    col.Item().Row(row =>
                    {
                        var maxMes = porMes.OrderByDescending(m => m.cantidad).First();
                        row.RelativeItem().Background(MoradoClaro).Border(1).BorderColor(BordeClaro)
                            .Padding(14).Column(c =>
                            {
                                c.Item().Text("Total eventos").FontSize(8).FontColor(TextoGris);
                                c.Item().Text($"{eventos.Count}").FontSize(16).Bold().FontColor(MoradoPrincipal);
                            });
                        row.ConstantItem(16);
                        row.RelativeItem().Background(VerdeClaro).Border(1).BorderColor("#B2DDD5")
                            .Padding(14).Column(c =>
                            {
                                c.Item().Text("Mes con más eventos").FontSize(8).FontColor(TextoGris);
                                c.Item().Text($"{maxMes.nombre} ({maxMes.cantidad})")
                                    .FontSize(16).Bold().FontColor(Verde);
                            });
                    });

                    col.Item().Background(FondoSeccion).Border(1).BorderColor(BordeClaro)
                        .Padding(15).Column(seccion =>
                        {
                            seccion.Spacing(10);
                            seccion.Item().Text("EVENTOS POR MES")
                                .Bold().FontSize(12).FontColor(MoradoPrincipal);
                            seccion.Item().LineHorizontal(1).LineColor(BordeClaro);
                            seccion.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(3);
                                    cols.RelativeColumn(1);
                                });
                                table.Header(header =>
                                {
                                    header.Cell().Element(EstiloEncabezado).Text("Mes");
                                    header.Cell().Element(EstiloEncabezado).AlignCenter().Text("Eventos");
                                });
                                foreach (var m in porMes)
                                {
                                    table.Cell().Element(EstiloCelda).Text(m.nombre);
                                    table.Cell().Element(EstiloCelda).AlignCenter()
                                        .Text(m.cantidad == 0 ? "—" : m.cantidad.ToString());
                                }
                            });
                        });

                    if (servicios.Any())
                    {
                        col.Item().Background(FondoSeccion).Border(1).BorderColor(BordeClaro)
                            .Padding(15).Column(seccion =>
                            {
                                seccion.Spacing(10);
                                seccion.Item().Text("SERVICIOS MÁS CONTRATADOS")
                                    .Bold().FontSize(12).FontColor(MoradoPrincipal);
                                seccion.Item().LineHorizontal(1).LineColor(BordeClaro);
                                seccion.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(3);
                                        cols.RelativeColumn(2);
                                        cols.RelativeColumn(1);
                                        cols.RelativeColumn(2);
                                    });
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(EstiloEncabezado).Text("Servicio");
                                        header.Cell().Element(EstiloEncabezado).Text("Categoría");
                                        header.Cell().Element(EstiloEncabezado).AlignCenter().Text("Frecuencia");
                                        header.Cell().Element(EstiloEncabezado).AlignRight().Text("Ingresos");
                                    });
                                    foreach (var s in servicios)
                                    {
                                        table.Cell().Element(EstiloCelda).Text(s.nombre);
                                        table.Cell().Element(EstiloCelda).Text(s.categoria);
                                        table.Cell().Element(EstiloCelda).AlignCenter().Text(s.frecuencia.ToString());
                                        table.Cell().Element(EstiloCelda).AlignRight().Text($"₡{s.ingresos:N2}");
                                    }
                                });
                            });
                    }
                });

                page.Footer().PaddingHorizontal(35).PaddingVertical(15)
                    .BorderTop(1).BorderColor(BordeClaro)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SIGESA · Sala de Eventos Mónica")
                                .Bold().FontSize(9).FontColor(MoradoPrincipal);
                            col.Item().Text("Reporte generado automáticamente.")
                                .FontSize(8).FontColor(TextoGris);
                        });
                        row.RelativeItem().AlignRight().AlignMiddle().Text(text =>
                        {
                            text.Span("Página ").FontSize(8).FontColor(TextoGris);
                            text.CurrentPageNumber().FontSize(8).FontColor(TextoGris);
                            text.Span(" de ").FontSize(8).FontColor(TextoGris);
                            text.TotalPages().FontSize(8).FontColor(TextoGris);
                        });
                    });
            });
        });

        return File(pdf.GeneratePdf(), "application/pdf",
            $"reporte-ocupacion-{anioConsulta}.pdf");
    }

    // ── Helpers compartidos ──────────────────────────────────────────
    private static IContainer EstiloEncabezado(IContainer container) =>
        container
            .Background(MoradoPrincipal)
            .PaddingVertical(8).PaddingHorizontal(7)
            .DefaultTextStyle(s => s.FontSize(9).Bold().FontColor(Colors.White));

    private static IContainer EstiloCelda(IContainer container) =>
        container
            .BorderBottom(1).BorderColor(BordeClaro)
            .PaddingVertical(7).PaddingHorizontal(7)
            .DefaultTextStyle(s => s.FontSize(9).FontColor(TextoOscuro));
}