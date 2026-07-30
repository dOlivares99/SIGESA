namespace WEB.Models.ViewModels;

public class OcupacionViewModel
{
    public int Anio { get; set; }
    public int TotalEventos { get; set; }
    public string MesMaximo { get; set; } = string.Empty;
    public int CantidadMesMaximo { get; set; }
    public List<MesOcupacionViewModel> PorMes { get; set; } = new();
    public List<ServicioOcupacionViewModel> Servicios { get; set; } = new();
}

public class MesOcupacionViewModel
{
    public int Mes { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class ServicioOcupacionViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int Frecuencia { get; set; }
    public decimal Ingresos { get; set; }
}