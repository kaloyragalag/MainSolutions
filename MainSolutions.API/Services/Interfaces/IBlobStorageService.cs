// MainSolutions.API/Services/Interfaces/IBlobStorageService.cs
namespace MainSolutions.API.Services.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobUrl, CancellationToken cancellationToken = default);
}
