namespace Core.Application.Interfaces.Services;
public interface IDocumentService
{
    Task<string> SaveAsync(string fileName, string folderName, Stream stream);
    Task<(byte[] fileData, string fileType, string fileName)> GetAsync(string documentName, string folderName);
}