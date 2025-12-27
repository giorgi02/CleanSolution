using System.IO.Compression;
using System.Text;

namespace Core.Shared.Helpers;

public static class JsonCompressor
{
    public static byte[] Compress(string json)
    {
        using var outputStream = new MemoryStream();
        using var brotliStream = new BrotliStream(outputStream, CompressionLevel.Optimal);

        var jsonBytes = Encoding.UTF8.GetBytes(json);
        brotliStream.Write(jsonBytes, 0, jsonBytes.Length);

        return outputStream.ToArray();
    }

    public static string Decompress(byte[] compressedJson)
    {
        using var inputStream = new MemoryStream(compressedJson);
        using var brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress);
        using var reader = new StreamReader(brotliStream);
        return reader.ReadToEnd();
    }
}
