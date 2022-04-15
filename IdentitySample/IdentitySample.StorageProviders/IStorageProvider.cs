namespace IdentitySample.StorageProviders;

public interface IStorageProvider
{
    Task SaveAsync(string path, Stream stream, bool overwrite = false);

    Task<Stream?> ReadAsStreamAsync(string path);

    async Task SaveAsync(string path, byte[] content, bool overwrite = false)
    {
        using var stream = new MemoryStream(content);
        await SaveAsync(path, stream, overwrite).ConfigureAwait(false);
    }

    async Task<byte[]?> ReadAsByteArrayAsync(string path)
    {
        using var stream = await ReadAsStreamAsync(path).ConfigureAwait(false);
        if (stream != null)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
            return memoryStream.ToArray();
        }

        return null;
    }

    Task DeleteAsync(string path);
}
