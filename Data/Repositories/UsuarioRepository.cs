using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Data.Repositories;

public interface IUsuarioRepository
{
    Task<IEnumerable<Usuario>> ReadAsync();
    Task<Usuario?> FindAsync(int id);
    Task<Usuario?> FindByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CreateAsync(Usuario entity);
    Task<bool> UpdateAsync(Usuario entity);
    Task<bool> DeleteAsync(Usuario entity);
    Task<bool> UpsertAsync(Usuario entity, bool isUpdating);
}

public class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(SIGESAContext context) : base(context) { }

    public new async Task<IEnumerable<Usuario>> ReadAsync() =>
        await DbContext.Set<Usuario>()
            .Include(u => u.Rol)
            .Where(u => u.Activo == true)
            .OrderBy(u => u.Nombre)
            .ToListAsync();

    public new async Task<Usuario?> FindAsync(int id) =>
        await DbContext.Set<Usuario>()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == id && u.Activo == true);

    public async Task<Usuario?> FindByEmailAsync(string email) =>
        await DbContext.Set<Usuario>()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == email && u.Activo == true);

    public async Task<bool> EmailExistsAsync(string email) =>
        await DbContext.Set<Usuario>().AnyAsync(u => u.Email == email);
}
