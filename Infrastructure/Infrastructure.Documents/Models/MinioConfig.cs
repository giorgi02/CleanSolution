namespace Infrastructure.Documents.Models;
internal class MinioConfig
{
    public MinioBucket MinioBucket { get; set; } = null!;
    public MinioClient MinioClient { get; set; } = null!;
}

internal class MinioBucket
{
    public string BucketName { get; set; } = null!;

}

internal class MinioClient
{
    public string? Endpoint { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
}