using Data.Repositories;
using Models.Entities;

namespace Business.Services;

public interface ICotizacionService
{
    Task<IEnumerable<Cotizacion>> ObtenerTodasAsync();
    Task<Cotizacion?> ObtenerPorIdAsync(int id);
    Task<bool> CrearAsync(int eventoId, DateTime? fechaVencimiento, int usuarioId);
    Task<bool> ActualizarEstadoAsync(int id, string estado, string? motivoRechazo,
                                     decimal total, DateTime? fechaVencimiento);
}

public class CotizacionService : ICotizacionService
{
    private readonly ICotizacionRepository _repo;
    private readonly IReservaRepository _reservaRepo;

    public CotizacionService(ICotizacionRepository repo, IReservaRepository reservaRepo)
    {
        _repo = repo;
        _reservaRepo = reservaRepo;
    }

    public async Task<IEnumerable<Cotizacion>> ObtenerTodasAsync() =>
        await _repo.ReadAsync();

    public async Task<Cotizacion?> ObtenerPorIdAsync(int id) =>
        await _repo.FindAsync(id);

    public async Task<bool> CrearAsync(int eventoId, DateTime? fechaVencimiento, int usuarioId)
    {
        var evento = await _reservaRepo.FindConDetallesAsync(eventoId);
        if (evento == null) return false;

        var totalServicios = evento.EventoServicios
            .Sum(es => es.Cantidad * es.PrecioAcordado);

        var total = evento.MontoTotal + totalServicios;

        var cotizacion = new Cotizacion
        {
            EventoId = eventoId,
            Total = total,
            Estado = "Borrador",
            FechaCreacion = DateTime.Now,
            FechaVencimiento = fechaVencimiento,
            UsuarioCreacion = usuarioId
        };

        return await _repo.CreateAsync(cotizacion);
    }

    public async Task<bool> ActualizarEstadoAsync(int id, string estado,
        string? motivoRechazo, decimal total, DateTime? fechaVencimiento)
    {
        var cotizacion = await _repo.FindAsync(id);
        if (cotizacion == null) return false;

        cotizacion.Estado = estado;
        cotizacion.MotivoRechazo = estado == "Rechazada" ? motivoRechazo : null;
        cotizacion.Total = total;
        cotizacion.FechaVencimiento = fechaVencimiento;

        return await _repo.UpdateAsync(cotizacion);
    }
}