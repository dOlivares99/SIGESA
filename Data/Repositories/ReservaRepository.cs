using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Data.Repositories;

public interface IReservaRepository
{
    Task<IEnumerable<Evento>> ReadAsync();
    Task<Evento?> FindAsync(int id);
    Task<Evento?> FindConDetallesAsync(int id);
    Task<bool> CreateAsync(Evento entity);
    Task<bool> UpdateAsync(Evento entity);
    void EliminarServicios(IEnumerable<EventoServicio> servicios);

    Task<bool> DeleteAsync(Evento entity);
}

public class ReservaRepository : RepositoryBase<Evento>, IReservaRepository
{
    public ReservaRepository(SIGESAContext context) : base(context) { }

    public new async Task<IEnumerable<Evento>> ReadAsync() =>
        await DbContext.Set<Evento>()
            .Include(e => e.Cliente)
            .Include(e => e.Paquete)
            .OrderByDescending(e => e.FechaEvento)
            .ToListAsync();

    public new async Task<Evento?> FindAsync(int id) =>
        await DbContext.Set<Evento>()
            .Include(e => e.Cliente)
            .Include(e => e.Paquete)
            .FirstOrDefaultAsync(e => e.EventoId == id);

    public async Task<Evento?> FindConDetallesAsync(int id) =>
        await DbContext.Set<Evento>()
            .Include(e => e.Cliente)
            .Include(e => e.Paquete)
            .Include(e => e.EventoServicios)
                .ThenInclude(es => es.Servicio)
            .FirstOrDefaultAsync(e => e.EventoId == id);

    public new async Task<bool> CreateAsync(Evento entity)
    {
        await DbContext.AddAsync(entity);
        return await DbContext.SaveChangesAsync() > 0;
    }

    public new async Task<bool> UpdateAsync(Evento entity)
    {
        DbContext.Update(entity);
        return await DbContext.SaveChangesAsync() > 0;
    }

    public void EliminarServicios(IEnumerable<EventoServicio> servicios)
    {
        DbContext.Set<EventoServicio>().RemoveRange(servicios);
    }


    public new async Task<bool> DeleteAsync(Evento entity)
    {
        DbContext.Remove(entity);
        return await DbContext.SaveChangesAsync() > 0;
    }
}