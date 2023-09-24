using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Minio;

namespace Infrastructure.Documents.Implementations;
internal class DocumentService : IDocumentService
{
    private readonly MinioClient _minio;
    private readonly string _bucketName;

    public DocumentService(MinioClient minio, IConfiguration configuration)
    {
        _minio = minio ?? throw new ArgumentNullException(nameof(minio));
        _bucketName = configuration["Minio:BucketName"] ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<string> SaveAsync(Stream stream, string fileName)
    {
        var pointIndex = fileName.LastIndexOf(".", StringComparison.Ordinal);

        var hash = Guid.NewGuid().ToString("N");

        var objectName = fileName.Insert(pointIndex, $"_{hash}_");

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType("application/octet-stream");

        await _minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

        return objectName;
    }

    public async Task<(byte[] fileData, string fileType)> GetAsync(string fileName)
    {
        var statObjectArgs = new StatObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName);

        await _minio.StatObjectAsync(statObjectArgs).ConfigureAwait(false); //if file not found throws exception

        var ms = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithCallbackStream((stream) => stream.CopyTo(ms));

        await _minio.GetObjectAsync(getObjectArgs).ConfigureAwait(false);

        ms.Position = 0;

        return (ms.ToArray(), GetContetnType(fileName));
    }

    private static string GetContetnType(string fileName)
    {
        var provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }
}