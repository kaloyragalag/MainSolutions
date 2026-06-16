// MainSolutions.API/Services/AzureBlobStorageService.cs
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MainSolutions.API.Options;
using MainSolutions.API.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace MainSolutions.API.Services;

public sealed class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;

    public AzureBlobStorageService(IOptions<AzureStorageOptions> options)
    {
        var settings = options.Value;
        _container = new BlobContainerClient(settings.ConnectionString, settings.ContainerName);
        _container.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var blobClient = _container.GetBlobClient(blobName);

        await blobClient.UploadAsync(
            content,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string blobUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobUrl)) return;

        var blobName = Path.GetFileName(new Uri(blobUrl).LocalPath);
        await _container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}