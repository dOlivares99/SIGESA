using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels;

public class PagoViewModel
{
    public int PagoId { get; set; }
    public int EventoId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string? Observacion { get; set; }
    public string? UrlComprobante { get; set; }
    public string TipoPagoNombre { get; set; } = string.Empty;
    public string MetodoPagoNombre { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
}

public class PagoFormViewModel
{
    [Required(ErrorMessage = "Seleccione un evento")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un evento")]
    public int EventoId { get; set; }

    [Required(ErrorMessage = "Seleccione el tipo de pago")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione el tipo de pago")]
    public int TipoPagoId { get; set; }

    [Required(ErrorMessage = "Seleccione el método de pago")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione el método de pago")]
    public int MetodoPagoId { get; set; }

    [Required(ErrorMessage = "Ingrese el monto")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "Ingrese la fecha del pago")]
    public DateTime FechaPago { get; set; } = DateTime.Today;

    [MaxLength(500)]
    public string? Observacion { get; set; }

    public IFormFile? Comprobante { get; set; }

    public List<EventoOpcionViewModel> Eventos { get; set; } = new();
    public List<CatalogoOpcionViewModel> TiposPago { get; set; } = new();
    public List<CatalogoOpcionViewModel> MetodosPago { get; set; } = new();
}

public class EventoOpcionViewModel
{
    public int EventoId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal SaldoPendiente { get; set; }
}

public class CatalogoOpcionViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
