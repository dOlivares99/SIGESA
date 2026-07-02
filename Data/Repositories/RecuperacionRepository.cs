using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Data.Repositories;

public interface IRecuperacionRepository
{
    Task GuardarTokenAsync(int usuarioId, string token, DateTime expiracion);
    Task<TokenRecuperacion?> BuscarTokenValidoAsync(string token);
    Task InvalidarTokenAsync(string token);
    Task LimpiarExpiradosAsync();
}

public class RecuperacionRepository : IRecuperacionRepository
{
    private readonly SIGESAContext _ctx;

    public RecuperacionRepository(SIGESAContext ctx)
    {
        _ctx = ctx;
    }

    public async Task GuardarTokenAsync(int usuarioId, string token, DateTime expiracion)
    {
        // Invalida tokens anteriores del mismo usuario
        var anteriores = await _ctx.Set<TokenRecuperacion>()
            .Where(t => t.UsuarioId == usuarioId && !t.Usado)
            .ToListAsync();
        anteriores.ForEach(t => t.Usado = true);

        _ctx.Set<TokenRecuperacion>().Add(new TokenRecuperacion
        {
            UsuarioId = usuarioId,
            Token = token,
            Expiracion = expiracion,
            Usado = false
        });

        await _ctx.SaveChangesAsync();
    }

    public async Task<TokenRecuperacion?> BuscarTokenValidoAsync(string token) =>
        await _ctx.Set<TokenRecuperacion>()
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t =>
                t.Token == token &&
                !t.Usado &&
                t.Expiracion > DateTime.UtcNow);

    public async Task InvalidarTokenAsync(string token)
    {
        var registro = await _ctx.Set<TokenRecuperacion>()
            .FirstOrDefaultAsync(t => t.Token == token);
        if (registro != null)
        {
            registro.Usado = true;
            await _ctx.SaveChangesAsync();
        }
    }

    public async Task LimpiarExpiradosAsync()
    {
        var expirados = await _ctx.Set<TokenRecuperacion>()
            .Where(t => t.Expiracion < DateTime.UtcNow)
            .ToListAsync();
        _ctx.Set<TokenRecuperacion>().RemoveRange(expirados);
        await _ctx.SaveChangesAsync();
    }
}