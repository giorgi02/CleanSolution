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
        _bucketName = configuration["Minio:BucketName"] ?? throw new ArgumentNullException("Minio:BucketName");
    }

    public async Task<string> SaveAsync(string fileName, string folderName, Stream stream)
    {
        var pointIndex = fileName.LastIndexOf(".");

        var hash = Guid.NewGuid().ToString("N");

        var objectName = fileName.Insert(pointIndex, $"_{hash}_");

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType("application/octet-stream");

        await _minio.PutObjectAsync(putObjectArgs);

        return objectName;
    }


    public async Task<(byte[] fileData, string fileType, string fileName)> GetAsync(string documentName, string folderName)
    {
        var statObjectArgs = new StatObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(documentName);

        await _minio.StatObjectAsync(statObjectArgs); //if file not found throws exception

        var ms = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(documentName)
            .WithCallbackStream((stream) => stream.CopyTo(ms));

        await _minio.GetObjectAsync(getObjectArgs);

        ms.Position = 0;

        return (ms.ToArray(), GetContetnType(documentName), documentName);
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