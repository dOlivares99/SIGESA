using Models.Entities;

namespace API.Services
{
    public interface ICotizacionPdfService
    {
        byte[] GenerarPdf(Cotizacion cotizacion);
    }
}
