using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Data.Repositories;

public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> ReadAsync();
    Task<Cliente?> FindAsync(int id);
    Task<Cliente?> FindConEventosAsync(int id);
    Task<bool> DocumentoExistsAsync(string documento, int? excludeId = null);
    Task<bool> CreateAsync(Cliente entity);
    Task<bool> UpdateAsync(Cliente entity);
}

public class ClienteRepository : RepositoryBase<Cliente>, IClienteRepository
{
    public ClienteRepository(SIGESAContext context) : base(context) { }

    public new async Task<IEnumerable<Cliente>> ReadAsync() =>
        await DbContext.Set<Cliente>()
            .OrderBy(c => c.Nombre)
            .ToListAsync();

    public new async Task<Cliente?> FindAsync(int id) =>
        await DbContext.Set<Cliente>()
            .FirstOrDefaultAsync(c => c.ClienteId == id);

    // Para el historial: trae el cliente con todos sus eventos y paquete
    public async Task<Cliente?> FindConEventosAsync(int id) =>
        await DbContext.Set<Cliente>()
            .Include(c => c.Eventos)
                .ThenInclude(e => e.Paquete)
            .FirstOrDefaultAsync(c => c.ClienteId == id);

    public async Task<bool> DocumentoExistsAsync(string documento, int? excludeId = null) =>
        await DbContext.Set<Cliente>()
            .AnyAsync(c => c.Documento == documento &&
                           (excludeId == null || c.ClienteId != excludeId));
}