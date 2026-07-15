using Data.Repositories;
using Models.Entities;

namespace Business.Services;

public interface IPagoService
{
    Task<IEnumerable<Pago>> ObtenerTodosAsync();
    Task<IEnumerable<Pago>> ObtenerPorEventoAsync(int eventoId);
    Task<Pago?> ObtenerPorIdAsync(int id);
    Task<bool> CrearAsync(CrearPagoDto dto, int usuarioId);
    Task<bool> ActualizarComprobanteAsync(int pagoId, string url);
    Task<bool> EliminarAsync(int id);
    Task<IEnumerable<TipoPago>> ObtenerTiposPagoAsync();
    Task<IEnumerable<MetodoPago>> ObtenerMetodosPagoAsync();

}

public record CrearPagoDto(
    int EventoId,
    int TipoPagoId,
    int MetodoPagoId,
    decimal Monto,
    DateTime FechaPago,
    string? Observacion,
    string? UrlComprobante
);

public class PagoService : IPagoService
{
    private readonly IPagoRepository _repo;
    private readonly IReservaRepository _reservaRepo;

    public PagoService(IPagoRepository repo, IReservaRepository reservaRepo)
    {
        _repo = repo;
        _reservaRepo = reservaRepo;
    }

    public async Task<IEnumerable<Pago>> ObtenerTodosAsync() =>
        await _repo.ReadAsync();

    public async Task<IEnumerable<Pago>> ObtenerPorEventoAsync(int eventoId) =>
        await _repo.ReadPorEventoAsync(eventoId);

    public async Task<Pago?> ObtenerPorIdAsync(int id) =>
        await _repo.FindAsync(id);

    public async Task<bool> CrearAsync(CrearPagoDto dto, int usuarioId)
    {
        // Validar que el evento existe y tiene saldo pendiente
        var evento = await _reservaRepo.FindAsync(dto.EventoId);
        if (evento == null) return false;

        var saldoPendiente = evento.MontoTotal - evento.MontoPagado;
        if (dto.Monto <= 0 || dto.Monto > saldoPendiente) return false;

        var pago = new Pago
        {
            EventoId = dto.EventoId,
            TipoPagoId = dto.TipoPagoId,
            MetodoPagoId = dto.MetodoPagoId,
            Monto = dto.Monto,
            FechaPago = dto.FechaPago,
            Observacion = dto.Observacion,
            UrlComprobante = dto.UrlComprobante,
            UsuarioId = usuarioId
        };

        // El trigger TR_Pago_ActualizarEvento recalcula MontoPagado y EstadoPago
        return await _repo.CreateAsync(pago);
    }

    public async Task<bool> ActualizarComprobanteAsync(int pagoId, string url)
    {
        var pago = await _repo.FindAsync(pagoId);
        if (pago == null) return false;
        pago.UrlComprobante = url;
        return await _repo.UpdateAsync(pago);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var pago = await _repo.FindAsync(id);
        if (pago == null) return false;
        return await _repo.DeleteAsync(pago);
    }
    public async Task<IEnumerable<TipoPago>> ObtenerTiposPagoAsync() =>
    await _repo.ReadTiposAsync();

    public async Task<IEnumerable<MetodoPago>> ObtenerMetodosPagoAsync() =>
        await _repo.ReadMetodosAsync();
}
