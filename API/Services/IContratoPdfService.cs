using Models.Entities;

namespace API.Services;

public interface IContratoPdfService
{
    byte[] GenerarPdf(Contrato contrato);
}
