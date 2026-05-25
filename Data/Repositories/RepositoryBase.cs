using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public interface IRepositoryBase<T>
{
    Task<bool> UpsertAsync(T entity, bool isUpdating);
    Task<bool> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> UpdateManyAsync(IEnumerable<T> entities);
    Task<bool> DeleteAsync(T entity);
    Task<IEnumerable<T>> ReadAsync();
    Task<T?> FindAsync(int id);
    Task<bool> ExistsAsync(T entity);
}

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    private readonly DbContext _context;
    protected readonly DbSet<T> DbSet;
    protected DbContext DbContext => _context;

    public RepositoryBase(DbContext context)
    {
        _context = context;
        DbSet = _context.Set<T>();
    }

    public async Task<bool> UpsertAsync(T entity, bool isUpdating) =>
        isUpdating ? await UpdateAsync(entity) : await CreateAsync(entity);

    public async Task<bool> CreateAsync(T entity)
    {
        try
        {
            await _context.AddAsync(entity);
            return await SaveAsync();
        }
        catch (Exception ex) { throw new ApplicationException("Error al crear registro.", ex); }
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        try
        {
            _context.Update(entity);
            return await SaveAsync();
        }
        catch (Exception ex) { throw new ApplicationException("Error al actualizar registro.", ex); }
    }

    public async Task<bool> UpdateManyAsync(IEnumerable<T> entities)
    {
        try
        {
            _context.UpdateRange(entities);
            return await SaveAsync();
        }
        catch (Exception ex) { throw new ApplicationException("Error al actualizar registros.", ex); }
    }

    public async Task<bool> DeleteAsync(T entity)
    {
        try
        {
            _context.Remove(entity);
            return await SaveAsync();
        }
        catch (Exception ex) { throw new ApplicationException("Error al eliminar registro.", ex); }
    }

    public async Task<IEnumerable<T>> ReadAsync()
    {
        try { return await _context.Set<T>().ToListAsync(); }
        catch (Exception ex) { throw new ApplicationException("Error al leer registros.", ex); }
    }

    public async Task<T?> FindAsync(int id)
    {
        try { return await _context.Set<T>().FindAsync(id); }
        catch (Exception ex) { throw new ApplicationException("Error al buscar registro.", ex); }
    }

    public async Task<bool> ExistsAsync(T entity)
    {
        try
        {
            var items = await ReadAsync();
            return items.Any(x => x.Equals(entity));
        }
        catch (Exception ex) { throw new ApplicationException("Error al verificar existencia.", ex); }
    }

    protected async Task<bool> SaveAsync()
    {
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
