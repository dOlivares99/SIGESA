using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace API.Services;

public interface IBlobStorageService
{
    Task<string?> SubirComprobanteAsync(Stream archivo, string nombreOriginal, string contentType);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;
    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png" };
    private static readonly string[] ContentTypesPermitidos = { "image/jpeg", "image/png" };

    public BlobStorageService(IConfiguration config)
    {
        var connectionString = config["AzureBlob:ConnectionString"]
            ?? throw new InvalidOperationException("Falta AzureBlob:ConnectionString");
        var containerName = config["AzureBlob:Container"] ?? "comprobantes";

        _container = new BlobContainerClient(connectionString, containerName);
        _container.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string?> SubirComprobanteAsync(Stream archivo, string nombreOriginal, string contentType)
    {
        var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();

        // Escenario 3: rechazar formatos no permitidos
        if (!ExtensionesPermitidas.Contains(extension)) return null;
        if (!ContentTypesPermitidos.Contains(contentType.ToLowerInvariant())) return null;

        var nombreBlob = $"{Guid.NewGuid()}{extension}";
        var blobClient = _container.GetBlobClient(nombreBlob);

        await blobClient.UploadAsync(archivo, new BlobHttpHeaders { ContentType = contentType });
        return blobClient.Uri.ToString();
    }
}
