using Data.Repositories;
using Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Business.Services;

public interface IUsuarioService
{
    Task<Usuario?> AutenticarAsync(string email, string password);
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task<bool> EmailExisteAsync(string email);
    Task<bool> CrearAsync(string nombre, string email, string password, int rolId);
    Task<bool> ActualizarAsync(int id, string nombre, string email, int rolId);
    Task<(bool Exito, string? Error)> CambiarActivoAsync(int id, bool activo, int? usuarioActualId = null);
}

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repo;

    public UsuarioService(IUsuarioRepository repo)
    {
        _repo = repo;
    }

    public async Task<Usuario?> AutenticarAsync(string email, string password)
    {
        var usuario = await _repo.FindByEmailAsync(email);
        if (usuario == null) return null;
        return VerificarPassword(password, usuario.PasswordHash) ? usuario : null;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id) =>
        await _repo.FindAsync(id);

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync() =>
        await _repo.ReadAsync();

    public async Task<bool> EmailExisteAsync(string email) =>
        await _repo.EmailExistsAsync(email);

    public async Task<bool> CrearAsync(string nombre, string email,
        string password, int rolId)
    {
        var usuario = new Usuario
        {
            Nombre        = nombre,
            Email         = email,
            PasswordHash  = HashPassword(password),
            RolId         = rolId,
            Activo        = true,
            FechaCreacion = DateTime.Now
        };
        return await _repo.CreateAsync(usuario);
    }

    public async Task<bool> ActualizarAsync(int id, string nombre,
        string email, int rolId)
    {
        var usuario = await _repo.FindAsync(id);
        if (usuario == null) return false;

        usuario.Nombre = nombre;
        usuario.Email  = email;
        usuario.RolId  = rolId;

        return await _repo.UpdateAsync(usuario);
    }

    public async Task<(bool Exito, string? Error)> CambiarActivoAsync(int id, bool activo, int? usuarioActualId = null)
    {
        var usuario = await _repo.FindAsync(id);
        if (usuario == null) return (false, null);

        if (!activo)
        {
            if (usuarioActualId == id)
                return (false, "No puedes archivarte a ti mismo.");

            if (usuario.Rol.Nombre == "Administrador")
            {
                var usuarios = await _repo.ReadAsync();
                var otrosAdminsActivos = usuarios.Any(u =>
                    u.Activo && u.UsuarioId != id && u.Rol.Nombre == "Administrador");
                if (!otrosAdminsActivos)
                    return (false, "No se puede archivar al único administrador activo.");
            }
        }

        usuario.Activo = activo;
        return (await _repo.UpdateAsync(usuario), null);
    }

    // ── Hash helpers ─────────────────────────────────────────
    public static string HashPassword(string password)
    {
        var salt  = RandomNumberGenerator.GetBytes(16);
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash  = SHA256.HashData([.. salt, .. bytes]);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool VerificarPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;
        var salt  = Convert.FromBase64String(parts[0]);
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash  = SHA256.HashData([.. salt, .. bytes]);
        return Convert.ToBase64String(hash) == parts[1];
    }
}
