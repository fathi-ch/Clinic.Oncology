using Clinic.Core.Configurations;
using Clinic.Core.Data;
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Services;

public class FileRepository : IFileRepository
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;
    private readonly DatabaseConfigurations _dbConfigs;

    public FileRepository(ISqliteDbConnectionFactory connectionFactory, DatabaseConfigurations dbConfigs)
    {
        _connectionFactory = connectionFactory;
        _dbConfigs = dbConfigs;
    }

    public async Task<bool> SaveFilesAsync(IFormFile file, string path, FileMode fileMode = FileMode.Create)
    {
        try
        {
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

    public async Task<bool> DeleteFilesAsync(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
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