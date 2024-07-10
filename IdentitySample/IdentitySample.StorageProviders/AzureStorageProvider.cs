using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;

namespace IdentitySample.StorageProviders;

internal class AzureStorageProvider(AzureStorageSettings settings) : IStorageProvider
{
    private readonly BlobServiceClient blobServiceClient = new BlobServiceClient(settings.ConnectionString);

    public async Task SaveAsync(string path, Stream stream, bool overwrite = false)
    {
        var blobClient = await GetBlobClientAsync(path, true).ConfigureAwait(false);

        if (!overwrite)
        {
            var blobExists = await blobClient.ExistsAsync().ConfigureAwait(false);
            if (blobExists)
            {
                throw new IOException($"The file {path} already exists.");
            }
        }

        stream.Position = 0;
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = MimeUtility.GetMimeMapping(path) }).ConfigureAwait(false);
    }

    public async Task<Stream?> ReadAsStreamAsync(string path)
    {
        var blobClient = await GetBlobClientAsync(path).ConfigureAwait(false);

        var blobExists = await blobClient.ExistsAsync().ConfigureAwait(false);
        if (!blobExists)
        {
            return null;
        }

        var stream = await blobClient.OpenReadAsync();
        return stream;
    }

    public async Task DeleteAsync(string path)
    {
        var (containerName, blobName) = ExtractContainerBlobName(path);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await blobContainerClient.DeleteBlobIfExistsAsync(blobName).ConfigureAwait(false);
    }

    private async Task<BlobClient> GetBlobClientAsync(string path, bool createIfNotExists = false)
    {
        var (containerName, blobName) = ExtractContainerBlobName(path);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None).ConfigureAwait(false);
        }

        var blobClient = blobContainerClient.GetBlobClient(blobName);
        return blobClient;
    }

    private (string ContainerName, string BlobName) ExtractContainerBlobName(string? path)
    {
        path = path?.Replace(@"\", "/") ?? string.Empty;

        // If a container name as been provided in the settings, use it.
        // Otherwise, extract the first folder name from the path.
        if (!string.IsNullOrWhiteSpace(settings.ContainerName))
        {
            return (settings.ContainerName.ToLowerInvariant(), path);
        }

        var root = Path.GetPathRoot(path);
        var fileNameWithoutRoot = path[(root ?? string.Empty).Length..];
        var parts = fileNameWithoutRoot.Split('/');

        var containerName = parts.First().ToLowerInvariant();
        var blobName = string.Join('/', parts.Skip(1));

        return (containerName, blobName);
    }
}
