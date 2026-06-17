using Data.Repositories;
using Models.Entities;

namespace Business.Services;

public interface IPaqueteService
{
    Task<IEnumerable<Paquete>> ObtenerTodosAsync();
    Task<IEnumerable<Paquete>> ObtenerActivosAsync();
    Task<Paquete?> ObtenerPorIdAsync(int id);
    Task<bool> NombreExisteAsync(string nombre, int? excludeId = null);
    Task<bool> TieneCotizacionesBorradorAsync(int paqueteId);
    Task<bool> CrearAsync(string nombre, string? descripcion, decimal precioBase,
                          int maxPersonas, int duracionHoras);
    Task<bool> ActualizarAsync(int id, string nombre, string? descripcion,
                               decimal precioBase, int maxPersonas, int duracionHoras);
    Task<bool> CambiarActivoAsync(int id, bool activo);
}

public class PaqueteService : IPaqueteService
{
    private readonly IPaqueteRepository _repo;

    public PaqueteService(IPaqueteRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Paquete>> ObtenerTodosAsync() =>
        await _repo.ReadAsync();

    public async Task<IEnumerable<Paquete>> ObtenerActivosAsync() =>
        await _repo.ReadActivosAsync();

    public async Task<Paquete?> ObtenerPorIdAsync(int id) =>
        await _repo.FindAsync(id);

    public async Task<bool> NombreExisteAsync(string nombre, int? excludeId = null) =>
        await _repo.NombreExistsAsync(nombre, excludeId);

    public async Task<bool> TieneCotizacionesBorradorAsync(int paqueteId) =>
        await _repo.TieneCotizacionesBorradorAsync(paqueteId);

    public async Task<bool> CrearAsync(string nombre, string? descripcion,
        decimal precioBase, int maxPersonas, int duracionHoras)
    {
        var paquete = new Paquete
        {
            Nombre = nombre,
            Descripcion = descripcion,
            PrecioBase = precioBase,
            MaxPersonas = maxPersonas,
            DuracionHoras = duracionHoras,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        return await _repo.CreateAsync(paquete);
    }

    public async Task<bool> ActualizarAsync(int id, string nombre, string? descripcion,
        decimal precioBase, int maxPersonas, int duracionHoras)
    {
        var paquete = await _repo.FindAsync(id);
        if (paquete == null) return false;

        paquete.Nombre = nombre;
        paquete.Descripcion = descripcion;
        paquete.PrecioBase = precioBase;
        paquete.MaxPersonas = maxPersonas;
        paquete.DuracionHoras = duracionHoras;

        return await _repo.UpdateAsync(paquete);
    }

    public async Task<bool> CambiarActivoAsync(int id, bool activo)
    {
        var paquete = await _repo.FindAsync(id);
        if (paquete == null) return false;

        paquete.Activo = activo;
        return await _repo.UpdateAsync(paquete);
    }
}