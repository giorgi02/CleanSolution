// @module documents
using Core.Application.Commons;

namespace Core.Application.Interfaces.Services;

public interface IDocumentService
{
    /// <summary>
    /// Stores the file and returns its unique object name (persist this name on the entity).
    /// </summary>
    Task<string> SaveAsync(FileContent file, CancellationToken cancellationToken = default);

    Task<(byte[] FileData, string ContentType)> GetAsync(string objectName, CancellationToken cancellationToken = default);
}
