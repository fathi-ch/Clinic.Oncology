using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Services;

public interface IFileRepository
{
    Task<bool> SaveFilesAsync(IFormFile file, string path, FileMode fileMode = FileMode.Create);
    Task<bool> DeleteFilesAsync(string path);
}