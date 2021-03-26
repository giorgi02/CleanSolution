using CleanSolution.Core.Application.Interfaces.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CleanSolution.Infrastructure.Files
{
    public class FileManager : IFileManager
    {
        private readonly string directoryAddress;
        public FileManager(IConfiguration configuration)
        {
            this.directoryAddress = configuration["Document:Address"];
        }

        public string SaveFile(IFormFile file)
        {
            // დირექტორიის შექმნა
            var directory = Path.Combine(directoryAddress, subDirectory);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // ფაილის საქაღალდეში გადატანა
            var filePath = Path.Combine(directory, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // ფაილისთვის სახელის გადარქმევა
            string fileNewName = $"{Path.GetFileNameWithoutExtension(file.FileName)} ({Guid.NewGuid()}){ Path.GetExtension(file.FileName).ToUpper()}";
            System.IO.File.Move(
               sourceFileName: $"{directoryAddress}\\{subDirectory }\\{file.FileName}",
               destFileName: $"{directoryAddress}\\{subDirectory }\\{fileNewName}"
               );

            return fileNewName;
        }
    }
}
