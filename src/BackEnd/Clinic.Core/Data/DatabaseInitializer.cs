using Clinic.Core.Configurations;
using Clinic.Core.Helpers;
using Dapper;

namespace Clinic.Core.Data;

public class DatabaseInitializer
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;
    private readonly DatabaseConfigurations _databaseConfig;
    public DatabaseInitializer(ISqliteDbConnectionFactory connectionFactory, DatabaseConfigurations databaseConfig)
    {
        _connectionFactory = connectionFactory;
        _databaseConfig = databaseConfig;
    }
    public async Task InitializeAsync()
    {
        SqlMapper.AddTypeHandler(new SqLiteGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        Guard.IsNotNull(_databaseConfig.DatabaseConfigFilePath, nameof(_databaseConfig.DatabaseConfigFilePath));

        using var connection = await _connectionFactory.CreateDbConnectionAsync();


        try
        {
            await connection.ExecuteAsync(await GetDataBaseConfigsFromFileAsync(Path.Combine(AppContext.BaseDirectory, _databaseConfig.DatabaseConfigFilePath)));
        }
        catch (Exception e)
        {
            
            throw;
        }
    }

    private static async Task<string?> GetDataBaseConfigsFromFileAsync(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }
}