namespace WEB.Models.ViewModels;

public class ReporteViewModel
{
    public DateTime Desde { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    public DateTime Hasta { get; set; } = DateTime.Now;
}

public class ReporteResultadoViewModel
{
    public bool SinDatos { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public decimal TotalIngresos { get; set; }
    public int TotalPagos { get; set; }
    public List<DesgloseViewModel> Desglose { get; set; } = new();
    public List<DetallepagoViewModel> Detalle { get; set; } = new();
}

public class DesgloseViewModel
{
    public string TipoEvento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}

public class DetallepagoViewModel
{
    public int PagoId { get; set; }
    public string FechaPago { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public string TipoPago { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}