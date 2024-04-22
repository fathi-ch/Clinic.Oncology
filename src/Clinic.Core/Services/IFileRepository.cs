using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Services;

public interface IFileRepository
{
    Task<bool> SaveFilesAsync(string file, string fileName, string visitId);
    Task<bool> DeleteFilesAsync(string fileName, string visitId);
}