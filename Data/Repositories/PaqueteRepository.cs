using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Data.Repositories;

public interface IPaqueteRepository
{
    Task<IEnumerable<Paquete>> ReadAsync();
    Task<IEnumerable<Paquete>> ReadActivosAsync();
    Task<Paquete?> FindAsync(int id);
    Task<bool> NombreExistsAsync(string nombre, int? excludeId = null);
    Task<bool> TieneCotizacionesBorradorAsync(int paqueteId);
    Task<bool> CreateAsync(Paquete entity);
    Task<bool> UpdateAsync(Paquete entity);
}

public class PaqueteRepository : RepositoryBase<Paquete>, IPaqueteRepository
{
    public PaqueteRepository(SIGESAContext context) : base(context) { }

    public new async Task<IEnumerable<Paquete>> ReadAsync() =>
        await DbContext.Set<Paquete>()
            .OrderBy(p => p.Nombre)
            .ToListAsync();

    public async Task<IEnumerable<Paquete>> ReadActivosAsync() =>
        await DbContext.Set<Paquete>()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

    public new async Task<Paquete?> FindAsync(int id) =>
        await DbContext.Set<Paquete>()
            .FirstOrDefaultAsync(p => p.PaqueteId == id);

    public async Task<bool> NombreExistsAsync(string nombre, int? excludeId = null) =>
        await DbContext.Set<Paquete>()
            .AnyAsync(p => p.Nombre == nombre &&
                           (excludeId == null || p.PaqueteId != excludeId));

    // Verifica si el paquete está en cotizaciones en estado Borrador
    // (a través de Eventos asociados)
    public async Task<bool> TieneCotizacionesBorradorAsync(int paqueteId) =>
        await DbContext.Set<Cotizacion>()
            .AnyAsync(c => c.Evento.PaqueteId == paqueteId &&
                           c.Estado == "Borrador");
}