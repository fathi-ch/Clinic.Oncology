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

    public async Task<bool> SaveFilesAsync(string file, string fileName,string visitId)
    {
        try
        {
            if(!Directory.Exists(Path.Combine(_dbConfigs.DocumentsPath, visitId))) {
                Directory.CreateDirectory(Path.Combine(_dbConfigs.DocumentsPath, visitId));
            }
            var pathf = Path.Combine(_dbConfigs.DocumentsPath, visitId, fileName);
            var bytes = Convert.FromBase64String(file.Split(',')[1]);
            File.WriteAllBytes(pathf, bytes);
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

    public async Task<bool> DeleteFilesAsync(string fileName,string visitId)
    {
        var currDirectory = Path.Combine(_dbConfigs.DocumentsPath,visitId,fileName);
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