namespace WEB.Models.ViewModels;

public class DashboardViewModel
{
    public int EventosMes { get; set; }
    public decimal IngresosMes { get; set; }
    public decimal SaldoPendienteTotal { get; set; }
    public int TotalClientes { get; set; }
    public bool TieneSaldosPendientes { get; set; }
    public string MesActual { get; set; } = string.Empty;

    public List<ProximoEventoViewModel> ProximosEventos { get; set; } = new();
    public List<AlertaSaldoViewModel> AlertasSaldo { get; set; } = new();
    public List<GraficaMesViewModel> GraficaMeses { get; set; } = new();
    public List<UltimoPagoViewModel> UltimosPagos { get; set; } = new();
    public List<EstadoEventoViewModel> PorEstado { get; set; } = new();
}

public class ProximoEventoViewModel
{
    public int EventoId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string FechaEvento { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int NumPersonas { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string PaqueteNombre { get; set; } = string.Empty;
}

public class AlertaSaldoViewModel
{
    public int EventoId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string FechaEvento { get; set; } = string.Empty;
    public decimal SaldoPendiente { get; set; }
    public decimal MontoTotal { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
}

public class GraficaMesViewModel
{
    public string Mes { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class UltimoPagoViewModel
{
    public int PagoId { get; set; }
    public decimal Monto { get; set; }
    public string FechaPago { get; set; } = string.Empty;
    public string TipoPago { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
}

public class EstadoEventoViewModel
{
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}