using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Repositories;

public interface IFileRepository
{
    Task<bool> SaveFilesAsync(IEnumerable<IFormFile> files, Guid id);
}