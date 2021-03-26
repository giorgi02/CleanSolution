using Microsoft.AspNetCore.Http;

namespace CleanSolution.Core.Application.Interfaces.Contracts
{
    public interface IFileManager
    {
        public string SaveFile(IFormFile file);
    }
}
