using Data.Repositories;
using Models.Entities;

namespace Business.Services;

public interface IReservaService
{
    Task<IEnumerable<Evento>> ObtenerTodosAsync();
    Task<Evento?> ObtenerPorIdAsync(int id);
    Task<Evento?> ObtenerConDetallesAsync(int id);
    Task<bool> CrearAsync(CrearReservaDto dto, int usuarioId);
    Task<bool> ActualizarAsync(int id, ActualizarReservaDto dto);
    Task<bool> EliminarAsync(int id);
}

public record CrearReservaDto(
    int ClienteId,
    int PaqueteId,
    string TipoEvento,
    DateOnly FechaEvento,
    int NumPersonas,
    decimal MontoTotal,
    string? Notas,
    List<EventoServicioDto> Servicios
);

public record ActualizarReservaDto(
    int ClienteId,
    int PaqueteId,
    string TipoEvento,
    DateOnly FechaEvento,
    int NumPersonas,
    string Estado,
    decimal MontoTotal,
    string? Notas,
    List<EventoServicioDto> Servicios
);

public record EventoServicioDto(int ServicioId, int Cantidad, decimal PrecioAcordado);

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _repo;

    public ReservaService(IReservaRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Evento>> ObtenerTodosAsync() =>
        await _repo.ReadAsync();

    public async Task<Evento?> ObtenerPorIdAsync(int id) =>
        await _repo.FindAsync(id);

    public async Task<Evento?> ObtenerConDetallesAsync(int id) =>
        await _repo.FindConDetallesAsync(id);

    public async Task<bool> CrearAsync(CrearReservaDto dto, int usuarioId)
    {
        var evento = new Evento
        {
            ClienteId = dto.ClienteId,
            PaqueteId = dto.PaqueteId,
            TipoEvento = dto.TipoEvento,
            FechaEvento = dto.FechaEvento,
            NumPersonas = dto.NumPersonas,
            MontoTotal = dto.MontoTotal,
            MontoPagado = 0,
            Estado = "Pendiente",
            EstadoPago = "Pendiente",
            Notas = dto.Notas,
            FechaCreacion = DateTime.Now,
            UsuarioCreacion = usuarioId,
            EventoServicios = dto.Servicios.Select(s => new EventoServicio
            {
                ServicioId = s.ServicioId,
                Cantidad = s.Cantidad,
                PrecioAcordado = s.PrecioAcordado
            }).ToList()
        };

        return await _repo.CreateAsync(evento);
    }

    public async Task<bool> ActualizarAsync(int id, ActualizarReservaDto dto)
    {
        var evento = await _repo.FindConDetallesAsync(id);
        if (evento == null) return false;

        evento.ClienteId = dto.ClienteId;
        evento.PaqueteId = dto.PaqueteId;
        evento.TipoEvento = dto.TipoEvento;
        evento.FechaEvento = dto.FechaEvento;
        evento.NumPersonas = dto.NumPersonas;
        evento.Estado = dto.Estado;
        evento.MontoTotal = dto.MontoTotal;
        evento.Notas = dto.Notas;

        evento.EventoServicios.Clear();
        foreach (var s in dto.Servicios)
        {
            evento.EventoServicios.Add(new EventoServicio
            {
                ServicioId = s.ServicioId,
                Cantidad = s.Cantidad,
                PrecioAcordado = s.PrecioAcordado
            });
        }

        return await _repo.UpdateAsync(evento);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var evento = await _repo.FindAsync(id);
        if (evento == null) return false;
        return await _repo.DeleteAsync(evento);
    }
}