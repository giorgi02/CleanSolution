namespace Core.Application.Commons;

/// <summary>
/// An uploaded file, independent of ASP.NET Core. Controllers convert IFormFile into this type,
/// so the Application layer never references Microsoft.AspNetCore.Http.
/// </summary>
public sealed record FileContent(Stream Content, string FileName, string ContentType, long Length);
