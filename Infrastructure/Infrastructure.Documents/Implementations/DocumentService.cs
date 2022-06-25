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
        _bucketName = configuration["Minio:MinioBucket:bucketName"];
    }

    public async Task<string> SaveAsync(string fileName, string folderName, Stream stream)
    {
        var pointIndex = fileName.LastIndexOf(".");

        var hash = Guid.NewGuid().ToString("N");

        var objectName = fileName.Insert(pointIndex, $"_{hash}_");

        PutObjectArgs putObjectArgs = new PutObjectArgs()
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
        var ms = new MemoryStream();

        await _minio.StatObjectAsync(_bucketName, folderName + "/" + documentName); //if file not found throws exception
        await _minio.GetObjectAsync(_bucketName, folderName + "/" + documentName, (stream) =>
        {
            stream.CopyTo(ms);
        });

        ms.Position = 0;
        var t = ms.ToArray();

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

    //public static byte[] ReadFully(Stream input)
    //{
    //    byte[] buffer = new byte[16 * 1024];
    //    using (MemoryStream ms = new MemoryStream())
    //    {
    //        int read;
    //        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
    //        {
    //            ms.Write(buffer, 0, read);
    //        }
    //        return ms.ToArray();
    //    }
    //}
}
