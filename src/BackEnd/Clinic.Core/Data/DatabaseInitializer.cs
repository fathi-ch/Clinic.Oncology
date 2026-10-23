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
            await AddMissingColumnsAsync(connection);
        }
        catch (Exception e)
        {
            
            throw;
        }
    }

    // CREATE TABLE IF NOT EXISTS never changes a table that already exists, so columns added after
    // the first release are added here. Existing visits become "PAID", as they were before billing types.
    private static async Task AddMissingColumnsAsync(System.Data.IDbConnection connection)
    {
        var visitColumns = (await connection.QueryAsync<string>("SELECT name FROM pragma_table_info('Visits');")).ToHashSet();
        if (!visitColumns.Contains("BillingType"))
            await connection.ExecuteAsync("ALTER TABLE Visits ADD COLUMN BillingType TEXT NOT NULL DEFAULT 'PAID';");
    }

    private static async Task<string?> GetDataBaseConfigsFromFileAsync(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }
}