using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Data.Repositories;

public interface IContratoRepository
{
    Task<IEnumerable<Contrato>> ReadAsync();
    Task<Contrato?> FindAsync(int id);
    Task<Contrato?> FindByCotizacionAsync(int cotizacionId);
    Task<bool> CreateAsync(Contrato entity);
    Task<bool> UpdateAsync(Contrato entity);
}

public class ContratoRepository : RepositoryBase<Contrato>, IContratoRepository
{
    public ContratoRepository(SIGESAContext context)
        : base(context)
    {
    }

    public new async Task<IEnumerable<Contrato>> ReadAsync()
    {
        return await DbContext.Set<Contrato>()
            .Include(c => c.Cotizacion)
                .ThenInclude(cot => cot.Evento)
                    .ThenInclude(e => e.Cliente)
            .Include(c => c.Cotizacion)
                .ThenInclude(cot => cot.Evento)
                    .ThenInclude(e => e.Paquete)
            .Include(c => c.Cotizacion)
                .ThenInclude(cot => cot.Evento)
                    .ThenInclude(e => e.EventoServicios)
                        .ThenInclude(es => es.Servicio)
            .Include(c => c.UsuarioCreacionNavigation)
            .OrderByDescending(c => c.FechaContrato)
            .ToListAsync();
    }

    public new async Task<Contrato?> FindAsync(int id)
    {
        return await DbContext.Set<Contrato>()
            .Include(c => c.Cotizacion)
                .ThenInclude(cot => cot.Evento)
                    .ThenInclude(e => e.Cliente)
            .Include(c => c.Cotizacion)
                .ThenInclude(cot => cot.Evento)
                    .ThenInclude(e => e.Paquete)
            .Include(c => c.Cotizacion)
                .ThenInclude(cot => cot.Evento)
                    .ThenInclude(e => e.EventoServicios)
                        .ThenInclude(es => es.Servicio)
            .Include(c => c.UsuarioCreacionNavigation)
            .FirstOrDefaultAsync(c => c.ContratoId == id);
    }

    public async Task<Contrato?> FindByCotizacionAsync(int cotizacionId)
    {
        return await DbContext.Set<Contrato>()
            .Include(c => c.Cotizacion)
                .ThenInclude(cot => cot.Evento)
                    .ThenInclude(e => e.Cliente)
            .Include(c => c.Cotizacion)
                .ThenInclude(cot => cot.Evento)
                    .ThenInclude(e => e.Paquete)
            .Include(c => c.Cotizacion)
                .ThenInclude(cot => cot.Evento)
                    .ThenInclude(e => e.EventoServicios)
                        .ThenInclude(es => es.Servicio)
            .Include(c => c.UsuarioCreacionNavigation)
            .FirstOrDefaultAsync(c => c.CotizacionId == cotizacionId);
    }
}
