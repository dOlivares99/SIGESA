namespace WEB.Models.ViewModels;

public class SaldoViewModel
{
    public int EventoId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string PaqueteNombre { get; set; } = string.Empty;
}