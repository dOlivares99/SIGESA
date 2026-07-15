using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Data.Repositories;

public interface IPagoRepository
{
    Task<IEnumerable<Pago>> ReadAsync();
    Task<IEnumerable<Pago>> ReadPorEventoAsync(int eventoId);
    Task<Pago?> FindAsync(int id);
    Task<bool> CreateAsync(Pago entity);
    Task<bool> UpdateAsync(Pago entity);
    Task<bool> DeleteAsync(Pago entity);
    Task<IEnumerable<TipoPago>> ReadTiposAsync();
    Task<IEnumerable<MetodoPago>> ReadMetodosAsync();

}

public class PagoRepository : RepositoryBase<Pago>, IPagoRepository
{
    public PagoRepository(SIGESAContext context) : base(context) { }

    public new async Task<IEnumerable<Pago>> ReadAsync() =>
        await DbContext.Set<Pago>()
            .Include(p => p.Evento)
                .ThenInclude(e => e.Cliente)
            .Include(p => p.TipoPago)
            .Include(p => p.MetodoPago)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

    public async Task<IEnumerable<Pago>> ReadPorEventoAsync(int eventoId) =>
        await DbContext.Set<Pago>()
            .Include(p => p.TipoPago)
            .Include(p => p.MetodoPago)
            .Where(p => p.EventoId == eventoId)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

    public new async Task<Pago?> FindAsync(int id) =>
        await DbContext.Set<Pago>()
            .Include(p => p.Evento)
                .ThenInclude(e => e.Cliente)
            .Include(p => p.TipoPago)
            .Include(p => p.MetodoPago)
            .FirstOrDefaultAsync(p => p.PagoId == id);

    public new async Task<bool> CreateAsync(Pago entity)
    {
        await DbContext.AddAsync(entity);
        return await DbContext.SaveChangesAsync() > 0;
    }

    public new async Task<bool> UpdateAsync(Pago entity)
    {
        DbContext.Update(entity);
        return await DbContext.SaveChangesAsync() > 0;
    }

    public new async Task<bool> DeleteAsync(Pago entity)
    {
        DbContext.Remove(entity);
        return await DbContext.SaveChangesAsync() > 0;
    }
    public async Task<IEnumerable<TipoPago>> ReadTiposAsync() =>
    await DbContext.Set<TipoPago>().Where(t => t.Activo).ToListAsync();

    public async Task<IEnumerable<MetodoPago>> ReadMetodosAsync() =>
        await DbContext.Set<MetodoPago>().Where(m => m.Activo).ToListAsync();
}
