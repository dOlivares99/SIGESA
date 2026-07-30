using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardApiController(SIGESAContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        var proximos30 = hoy.AddDays(30);
        var inicioAnio = new DateOnly(hoy.Year, 1, 1);

        // KPI 1 — Eventos del mes
        var eventosMes = await context.Eventos
            .CountAsync(e => e.FechaEvento >= inicioMes &&
                             e.FechaEvento <= finMes &&
                             e.Estado != "Cancelada");

        // KPI 2 — Ingresos del mes
        var ingresosMes = await context.Pagos
            .Where(p => DateOnly.FromDateTime(p.FechaPago) >= inicioMes &&
                        DateOnly.FromDateTime(p.FechaPago) <= finMes)
            .SumAsync(p => (decimal?)p.Monto) ?? 0;

        // KPI 3 — Saldo pendiente total
        var saldoPendienteTotal = await context.Eventos
            .Where(e => e.Estado != "Cancelada" && e.EstadoPago != "Saldado")
            .SumAsync(e => (decimal?)e.SaldoPendiente) ?? 0;

        // KPI 4 — Clientes registrados
        var totalClientes = await context.Clientes.CountAsync();

        // Próximos 5 eventos
        var proximosEventos = await context.Eventos
            .Include(e => e.Cliente)
            .Include(e => e.Paquete)
            .Where(e => e.FechaEvento >= hoy &&
                        e.FechaEvento <= proximos30 &&
                        e.Estado != "Cancelada")
            .OrderBy(e => e.FechaEvento)
            .Take(5)
            .Select(e => new
            {
                e.EventoId,
                e.TipoEvento,
                e.FechaEvento,
                e.Estado,
                e.NumPersonas,
                e.MontoTotal,
                e.SaldoPendiente,
                ClienteNombre = e.Cliente.Nombre,
                PaqueteNombre = e.Paquete.Nombre
            })
            .ToListAsync();

        // Alertas — Eventos próximos con saldo pendiente
        var alertasSaldo = await context.Eventos
            .Include(e => e.Cliente)
            .Where(e => e.FechaEvento >= hoy &&
                        e.FechaEvento <= proximos30 &&
                        e.Estado != "Cancelada" &&
                        e.SaldoPendiente > 0)
            .OrderBy(e => e.FechaEvento)
            .Select(e => new
            {
                e.EventoId,
                e.TipoEvento,
                e.FechaEvento,
                e.SaldoPendiente,
                e.MontoTotal,
                ClienteNombre = e.Cliente.Nombre
            })
            .ToListAsync();

        // Gráfica — Eventos por mes del año actual
        var eventosPorMes = await context.Eventos
            .Where(e => e.FechaEvento.Year == hoy.Year && e.Estado != "Cancelada")
            .GroupBy(e => e.FechaEvento.Month)
            .Select(g => new { mes = g.Key, cantidad = g.Count() })
            .ToListAsync();

        var graficaMeses = Enumerable.Range(1, 12).Select(m => new
        {
            mes = new DateTime(hoy.Year, m, 1).ToString("MMM"),
            cantidad = eventosPorMes.FirstOrDefault(e => e.mes == m)?.cantidad ?? 0
        }).ToList();

        // Últimos 5 pagos
        var ultimosPagos = await context.Pagos
            .Include(p => p.Evento).ThenInclude(e => e.Cliente)
            .Include(p => p.TipoPago)
            .OrderByDescending(p => p.FechaPago)
            .Take(5)
            .Select(p => new
            {
                p.PagoId,
                p.Monto,
                p.FechaPago,
                TipoPago = p.TipoPago.Nombre,
                ClienteNombre = p.Evento.Cliente.Nombre,
                TipoEvento = p.Evento.TipoEvento
            })
            .ToListAsync();

        // Distribución por estado
        var porEstado = await context.Eventos
            .Where(e => e.FechaEvento.Year == hoy.Year)
            .GroupBy(e => e.Estado)
            .Select(g => new { estado = g.Key, cantidad = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            eventosMes,
            ingresosMes,
            saldoPendienteTotal,
            totalClientes,
            proximosEventos,
            alertasSaldo,
            tieneSaldosPendientes = alertasSaldo.Any(),
            graficaMeses,
            ultimosPagos,
            porEstado,
            mesActual = inicioMes.ToString("MMMM yyyy")
        });
    }
}