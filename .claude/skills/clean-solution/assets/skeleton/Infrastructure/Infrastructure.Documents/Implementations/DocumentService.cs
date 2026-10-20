using Core.Application.Commons;
using Core.Application.Interfaces.Services;
using Infrastructure.Documents.Models;
using Microsoft.AspNetCore.StaticFiles;
using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.Documents.Implementations;

internal sealed class DocumentService(IMinioClient minio, MinioSettings settings) : IDocumentService
{
    private string BucketName => settings.MinioBucket.BucketName;

    public async Task<string> SaveAsync(FileContent file, CancellationToken cancellationToken = default)
    {
        // unique object name that keeps the original name and extension: photo.png => photo_<guid>.png
        var extension = Path.GetExtension(file.FileName);
        var objectName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid():N}{extension}";

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName)
            .WithStreamData(file.Content)
            .WithObjectSize(file.Length)
            .WithContentType(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);

        await minio.PutObjectAsync(putObjectArgs, cancellationToken).ConfigureAwait(false);

        return objectName;
    }

    public async Task<(byte[] FileData, string ContentType)> GetAsync(string objectName, CancellationToken cancellationToken = default)
    {
        var statObjectArgs = new StatObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName);

        await minio.StatObjectAsync(statObjectArgs, cancellationToken).ConfigureAwait(false); // throws when the object does not exist

        using var ms = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream => stream.CopyTo(ms));

        await minio.GetObjectAsync(getObjectArgs, cancellationToken).ConfigureAwait(false);

        return (ms.ToArray(), GetContentType(objectName));
    }

    private static string GetContentType(string fileName) =>
        new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType)
            ? contentType
            : "application/octet-stream";
}
