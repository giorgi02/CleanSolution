using CleanSolution.Core.Application.Interfaces.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CleanSolution.Infrastructure.Files;
public class FileManager : IFileManager
{
    private readonly string directory;
    public FileManager(IConfiguration configuration)
    {
        this.directory = configuration["Document:Address"];
    }

    public string SaveFile(IFormFile file)
    {
        // დირექტორიის შექმნა
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        // ფაილის საქაღალდეში გადატანა
        var filePath = Path.Combine(directory, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return Path.GetFileNameWithoutExtension(file.FileName);
    }
}