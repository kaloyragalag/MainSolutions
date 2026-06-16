// MainSolutions.API/Options/AzureStorageOptions.cs
namespace MainSolutions.API.Options;

public sealed class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";

    public string ConnectionString { get; init; } = string.Empty;
    public string ContainerName { get; init; } = "product-images";
}