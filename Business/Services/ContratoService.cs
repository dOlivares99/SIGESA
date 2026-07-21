using Data.Repositories;
using Models.Entities;

namespace Business.Services;

public interface IContratoService
{
    Task<IEnumerable<Contrato>> ObtenerTodosAsync();
    Task<Contrato?> ObtenerPorIdAsync(int id);
    Task<Contrato?> ObtenerPorCotizacionAsync(int cotizacionId);

    Task<(bool Exito, string Mensaje, Contrato? Contrato)> CrearAsync(
        int cotizacionId,
        string? observaciones,
        int usuarioId);

    Task<bool> ActualizarAsync(
        int id,
        string estado,
        string? observaciones);
}

public class ContratoService : IContratoService
{
    private readonly IContratoRepository _contratoRepo;
    private readonly ICotizacionRepository _cotizacionRepo;

    public ContratoService(
        IContratoRepository contratoRepo,
        ICotizacionRepository cotizacionRepo)
    {
        _contratoRepo = contratoRepo;
        _cotizacionRepo = cotizacionRepo;
    }

    public async Task<IEnumerable<Contrato>> ObtenerTodosAsync()
    {
        return await _contratoRepo.ReadAsync();
    }

    public async Task<Contrato?> ObtenerPorIdAsync(int id)
    {
        return await _contratoRepo.FindAsync(id);
    }

    public async Task<Contrato?> ObtenerPorCotizacionAsync(int cotizacionId)
    {
        return await _contratoRepo.FindByCotizacionAsync(cotizacionId);
    }

    public async Task<(bool Exito, string Mensaje, Contrato? Contrato)> CrearAsync(
        int cotizacionId,
        string? observaciones,
        int usuarioId)
    {
        var cotizacion = await _cotizacionRepo.FindAsync(cotizacionId);

        if (cotizacion == null)
        {
            return (
                false,
                "La cotización indicada no existe.",
                null);
        }

        var estadoCotizacion =
            cotizacion.Estado?.Trim() ?? string.Empty;

        if (!estadoCotizacion.Equals(
                "Aceptada",
                StringComparison.OrdinalIgnoreCase)
            && !estadoCotizacion.Equals(
                "Aprobada",
                StringComparison.OrdinalIgnoreCase))
        {
            return (
                false,
                "Solo se puede generar un contrato para una cotización aceptada o aprobada.",
                null);
        }

        var contratoExistente =
            await _contratoRepo.FindByCotizacionAsync(cotizacionId);

        if (contratoExistente != null)
        {
            return (
                false,
                "Ya existe un contrato para esta cotización.",
                contratoExistente);
        }

        var contrato = new Contrato
        {
            CotizacionId = cotizacionId,
            NumeroContrato = GenerarNumeroContrato(cotizacionId),
            FechaContrato = DateTime.Now,
            Estado = "Pendiente",
            Observaciones = observaciones,
            RutaPdf = null,
            UsuarioCreacion = usuarioId
        };

        var creado = await _contratoRepo.CreateAsync(contrato);

        if (!creado)
        {
            return (
                false,
                "No se pudo crear el contrato.",
                null);
        }

        return (
            true,
            "Contrato creado correctamente.",
            contrato);
    }

    public async Task<bool> ActualizarAsync(
        int id,
        string estado,
        string? observaciones)
    {
        var contrato = await _contratoRepo.FindAsync(id);

        if (contrato == null)
        {
            return false;
        }

        contrato.Estado = estado;
        contrato.Observaciones = observaciones;

        return await _contratoRepo.UpdateAsync(contrato);
    }

    private static string GenerarNumeroContrato(int cotizacionId)
    {
        return $"CONT-{DateTime.Now:yyyy}-{cotizacionId:D5}";
    }
}