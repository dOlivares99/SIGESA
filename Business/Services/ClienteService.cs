using Data.Repositories;
using Models.Entities;

namespace Business.Services;

public interface IClienteService
{
    Task<IEnumerable<Cliente>> ObtenerTodosAsync();
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<Cliente?> ObtenerConHistorialAsync(int id);
    Task<bool> DocumentoExisteAsync(string documento, int? excludeId = null);
    Task<bool> CrearAsync(string nombre, string documento, string telefono, string? email);
    Task<bool> ActualizarAsync(int id, string nombre, string documento, string telefono, string? email);
}

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repo;

    public ClienteService(IClienteRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Cliente>> ObtenerTodosAsync() =>
        await _repo.ReadAsync();

    public async Task<Cliente?> ObtenerPorIdAsync(int id) =>
        await _repo.FindAsync(id);

    public async Task<Cliente?> ObtenerConHistorialAsync(int id) =>
        await _repo.FindConEventosAsync(id);

    public async Task<bool> DocumentoExisteAsync(string documento, int? excludeId = null) =>
        await _repo.DocumentoExistsAsync(documento, excludeId);

    public async Task<bool> CrearAsync(string nombre, string documento,
        string telefono, string? email)
    {
        var cliente = new Cliente
        {
            Nombre = nombre,
            Documento = documento,
            Telefono = telefono,
            Email = email,
            FechaRegistro = DateTime.Now
        };
        return await _repo.CreateAsync(cliente);
    }

    public async Task<bool> ActualizarAsync(int id, string nombre, string documento,
        string telefono, string? email)
    {
        var cliente = await _repo.FindAsync(id);
        if (cliente == null) return false;

        cliente.Nombre = nombre;
        cliente.Documento = documento;
        cliente.Telefono = telefono;
        cliente.Email = email;

        return await _repo.UpdateAsync(cliente);
    }
}