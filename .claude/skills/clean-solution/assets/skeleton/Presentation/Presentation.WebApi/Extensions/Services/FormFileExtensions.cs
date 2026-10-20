using Core.Application.Commons;

namespace Presentation.WebApi.Extensions.Services;

public static class FormFileExtensions
{
    /// <summary>
    /// IFormFile stays in the Presentation layer; use cases receive the framework-free FileContent.
    /// </summary>
    public static FileContent ToFileContent(this IFormFile file) =>
        new(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
}
