namespace Infrastructure.Documents.Models;

/// <summary>
/// Bound from the "MinioConfigs" configuration section.
/// </summary>
internal sealed class MinioSettings
{
    public MinioBucketOptions MinioBucket { get; set; } = null!;
    public MinioClientOptions MinioClient { get; set; } = null!;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(MinioBucket?.BucketName, "MinioConfigs:MinioBucket:BucketName");
        ArgumentException.ThrowIfNullOrWhiteSpace(MinioClient?.Endpoint, "MinioConfigs:MinioClient:Endpoint");
        ArgumentException.ThrowIfNullOrWhiteSpace(MinioClient?.AccessKey, "MinioConfigs:MinioClient:AccessKey");
        ArgumentException.ThrowIfNullOrWhiteSpace(MinioClient?.SecretKey, "MinioConfigs:MinioClient:SecretKey");
    }
}

internal sealed class MinioBucketOptions
{
    public string BucketName { get; set; } = null!;
}

internal sealed class MinioClientOptions
{
    public string Endpoint { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
}
