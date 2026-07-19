using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Data.Repositories;

public interface ICotizacionRepository
{
    Task<IEnumerable<Cotizacion>> ReadAsync();
    Task<Cotizacion?> FindAsync(int id);
    Task<bool> CreateAsync(Cotizacion entity);
    Task<bool> UpdateAsync(Cotizacion entity);
}

public class CotizacionRepository : RepositoryBase<Cotizacion>, ICotizacionRepository
{
    public CotizacionRepository(SIGESAContext context) : base(context) { }

    public new async Task<IEnumerable<Cotizacion>> ReadAsync() =>
        await DbContext.Set<Cotizacion>()
            .Include(c => c.Evento)
                .ThenInclude(e => e.Cliente)
            .Include(c => c.Evento)
                .ThenInclude(e => e.Paquete)
            .Include(c => c.Evento)
                .ThenInclude(e => e.EventoServicios)
                    .ThenInclude(es => es.Servicio)
            .Include(c => c.UsuarioCreacionNavigation)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();

    public new async Task<Cotizacion?> FindAsync(int id) =>
        await DbContext.Set<Cotizacion>()
            .Include(c => c.Evento)
                .ThenInclude(e => e.Cliente)
            .Include(c => c.Evento)
                .ThenInclude(e => e.Paquete)
            .Include(c => c.Evento)
                .ThenInclude(e => e.EventoServicios)
                    .ThenInclude(es => es.Servicio)
            .FirstOrDefaultAsync(c => c.CotizacionId == id);
}