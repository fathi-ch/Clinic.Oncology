using Clinic.Core.Configurations;
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Services;

public class FileRepository : IFileRepository
{
    private readonly DatabaseConfigurations _dbConfigs;

    public FileRepository(DatabaseConfigurations dbConfigs)
    {
        _dbConfigs = dbConfigs;
    }

    public async Task<bool> SaveFilesAsync(IFormFile file, string fileName, FileMode fileMode = FileMode.Create)
    {
        try
        {
            var path = Path.Combine(_dbConfigs.GetFullSaveFolderPathForDocuments(fileName), fileName);

            await using var fileStream = new FileStream(path, fileMode);
            await file.CopyToAsync(fileStream);
        }
        catch (IOException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (Exception e)
        {
            //Logging to be added
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteFilesAsync(string fileName)
    {
        var currDirectory = _dbConfigs.GetFullSaveFolderPathForDocuments(fileName);
        try
        {
            var path = Path.Combine(currDirectory, fileName);
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }

            if (Directory.GetFiles(currDirectory).Length == 0)
            {
                await Task.Run(() => Directory.Delete(currDirectory));
            }
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }
}