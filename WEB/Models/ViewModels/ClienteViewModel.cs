using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels;

public class ClienteViewModel
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class ClienteFormViewModel
{
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento es obligatorio.")]
    [MaxLength(20)]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    [MaxLength(150)]
    public string? Email { get; set; }
}

// Para la vista de historial
public class ClienteHistorialViewModel
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<EventoResumenViewModel> Eventos { get; set; } = new();
}

public class EventoResumenViewModel
{
    public int EventoId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string PaqueteNombre { get; set; } = string.Empty;
}