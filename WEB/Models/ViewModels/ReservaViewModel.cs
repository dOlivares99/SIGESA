using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels;

public class ReservaViewModel
{
    public int EventoId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string PaqueteNombre { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public int NumPersonas { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
}

public class ReservaFormViewModel
{
    public int EventoId { get; set; }

    [Required(ErrorMessage = "El cliente es obligatorio.")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "El paquete es obligatorio.")]
    public int PaqueteId { get; set; }

    [Required(ErrorMessage = "El tipo de evento es obligatorio.")]
    [MaxLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    public string TipoEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateTime FechaEvento { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "El número de personas es obligatorio.")]
    [Range(1, 10000, ErrorMessage = "Debe ser al menos 1 persona.")]
    public int NumPersonas { get; set; } = 1;

    [Required(ErrorMessage = "El monto total es obligatorio.")]
    [Range(0, 99999999.99, ErrorMessage = "Ingrese un monto válido.")]
    public decimal MontoTotal { get; set; }

    public string Estado { get; set; } = "Pendiente";

    [MaxLength(1000)]
    public string? Notas { get; set; }

    public List<ServicioSeleccionadoViewModel> ServiciosSeleccionados { get; set; } = new();
    public List<ClienteOpcionViewModel> Clientes { get; set; } = new();
    public List<PaqueteOpcionViewModel> Paquetes { get; set; } = new();
    public List<ServicioOpcionViewModel> ServiciosDisponibles { get; set; } = new();
}

public class ServicioSeleccionadoViewModel
{
    public int ServicioId { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal PrecioAcordado { get; set; }
}

public class ClienteOpcionViewModel
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
}

public class PaqueteOpcionViewModel
{
    public int PaqueteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public int MaxPersonas { get; set; }
}

public class ServicioOpcionViewModel
{
    public int ServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
}