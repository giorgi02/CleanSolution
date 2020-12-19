using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CleanSolution.Core.Application.Interfaces.Contracts
{
    public interface IFileManager
    {
        public void SaveFile(List<IFormFile> files);
    }
}
