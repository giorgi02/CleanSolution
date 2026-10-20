using System.IO.Compression;
using System.Text;

namespace Core.Shared.Helpers;

/// <summary>
/// Brotli-compresses JSON into a byte[] (e.g. to store large JSON payloads as a BLOB column).
/// </summary>
public static class JsonCompressor
{
    public static byte[] Compress(string json)
    {
        using var outputStream = new MemoryStream();

        // the Brotli stream must be disposed (flushed) before reading the output buffer
        using (var brotliStream = new BrotliStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
        {
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            brotliStream.Write(jsonBytes, 0, jsonBytes.Length);
        }

        return outputStream.ToArray();
    }

    public static string Decompress(byte[] compressedJson)
    {
        using var inputStream = new MemoryStream(compressedJson);
        using var brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress);
        using var reader = new StreamReader(brotliStream, Encoding.UTF8);

        return reader.ReadToEnd();
    }
}
