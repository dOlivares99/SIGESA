using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Data.Repositories;

public interface IServicioRepository
{
    Task<IEnumerable<Servicio>> ReadAsync();
    Task<IEnumerable<Servicio>> ReadActivosAsync();
    Task<Servicio?> FindAsync(int id);
    Task<bool> NombreExistsAsync(string nombre, int? excludeId = null);
    Task<bool> CreateAsync(Servicio entity);
    Task<bool> UpdateAsync(Servicio entity);
}

public class ServicioRepository : RepositoryBase<Servicio>, IServicioRepository
{
    public ServicioRepository(SIGESAContext context) : base(context) { }

    public new async Task<IEnumerable<Servicio>> ReadAsync() =>
        await DbContext.Set<Servicio>()
            .OrderBy(s => s.Categoria)
            .ThenBy(s => s.Nombre)
            .ToListAsync();

    public async Task<IEnumerable<Servicio>> ReadActivosAsync() =>
        await DbContext.Set<Servicio>()
            .Where(s => s.Activo)
            .OrderBy(s => s.Categoria)
            .ThenBy(s => s.Nombre)
            .ToListAsync();

    public new async Task<Servicio?> FindAsync(int id) =>
        await DbContext.Set<Servicio>()
            .FirstOrDefaultAsync(s => s.ServicioId == id);

    public async Task<bool> NombreExistsAsync(string nombre, int? excludeId = null) =>
        await DbContext.Set<Servicio>()
            .AnyAsync(s => s.Nombre == nombre &&
                           (excludeId == null || s.ServicioId != excludeId));
}