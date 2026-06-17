using Data.Repositories;
using Models.Entities;

namespace Business.Services;

public interface IServicioService
{
    Task<IEnumerable<Servicio>> ObtenerTodosAsync();
    Task<IEnumerable<Servicio>> ObtenerActivosAsync();
    Task<Servicio?> ObtenerPorIdAsync(int id);
    Task<bool> NombreExisteAsync(string nombre, int? excludeId = null);
    Task<bool> CrearAsync(string nombre, string categoria, decimal precioUnitario);
    Task<bool> ActualizarAsync(int id, string nombre, string categoria, decimal precioUnitario);
    Task<bool> CambiarActivoAsync(int id, bool activo);
}

public class ServicioService : IServicioService
{
    private readonly IServicioRepository _repo;

    public ServicioService(IServicioRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Servicio>> ObtenerTodosAsync() =>
        await _repo.ReadAsync();

    public async Task<IEnumerable<Servicio>> ObtenerActivosAsync() =>
        await _repo.ReadActivosAsync();

    public async Task<Servicio?> ObtenerPorIdAsync(int id) =>
        await _repo.FindAsync(id);

    public async Task<bool> NombreExisteAsync(string nombre, int? excludeId = null) =>
        await _repo.NombreExistsAsync(nombre, excludeId);

    public async Task<bool> CrearAsync(string nombre, string categoria, decimal precioUnitario)
    {
        var servicio = new Servicio
        {
            Nombre = nombre,
            Categoria = categoria,
            PrecioUnitario = precioUnitario,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        return await _repo.CreateAsync(servicio);
    }

    public async Task<bool> ActualizarAsync(int id, string nombre, string categoria,
        decimal precioUnitario)
    {
        var servicio = await _repo.FindAsync(id);
        if (servicio == null) return false;

        servicio.Nombre = nombre;
        servicio.Categoria = categoria;
        servicio.PrecioUnitario = precioUnitario;

        return await _repo.UpdateAsync(servicio);
    }

    public async Task<bool> CambiarActivoAsync(int id, bool activo)
    {
        var servicio = await _repo.FindAsync(id);
        if (servicio == null) return false;

        servicio.Activo = activo;
        return await _repo.UpdateAsync(servicio);
    }
}