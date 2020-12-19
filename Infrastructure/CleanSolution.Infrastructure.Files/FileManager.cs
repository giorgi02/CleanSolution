using CleanSolution.Core.Application.Interfaces.Contracts;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CleanSolution.Infrastructure.Files
{
    public class FileManager : IFileManager
    {
        public void SaveFile(List<IFormFile> files)
        {
            throw new System.NotImplementedException();
        }
    }
}
