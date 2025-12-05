namespace Core.Application.Interfaces.Services;

public interface IDocumentService
{
    Task<string> SaveAsync(Stream stream, string fileName);
    Task<(byte[] fileData, string fileType)> GetAsync(string fileName);
}