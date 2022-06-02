using Microsoft.AspNetCore.Http;

namespace CleanSolution.Core.Application.Interfaces.Services;
public interface IFileManager
{
    string SaveFile(IFormFile file);
}