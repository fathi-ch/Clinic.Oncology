using System.Data;
using Clinic.Core.Configurations;
using Clinic.Core.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Clinic.Core.Data;

public class SqliteDbConnectionFactory : ISqliteDbConnectionFactory
{
    private string _connectionString;
    private readonly DatabaseConfigurations _databaseConfig;

    public SqliteDbConnectionFactory(DatabaseConfigurations databaseConfig)
    {
        _databaseConfig = databaseConfig;
        var dbSettings = _databaseConfig;
        Guard.IsNotNull(dbSettings.DatabasePath, nameof(dbSettings.DatabasePath));
        Guard.IsNotNull(dbSettings.DatabaseName, nameof(dbSettings.DatabaseName));
        Guard.IsNotNull(dbSettings.DocumentsPath, nameof(dbSettings.DocumentsPath));

        var fullDatabasePath = Path.Combine(dbSettings.DatabasePath, dbSettings.DatabaseName);
        PrepareDataWorkingDirectories(dbSettings);

        _connectionString = $@"Data Source={fullDatabasePath}";
    }

    public async Task<IDbConnection> CreateDbConnectionAsync()
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static void PrepareDataWorkingDirectories(DatabaseConfigurations dbSettings)
    {
        try
        {
            Directory.CreateDirectory(dbSettings.DatabasePath);
            Directory.CreateDirectory(Path.Combine(dbSettings.DatabasePath, dbSettings.DocumentsPath));
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create directory: " + ex.Message, ex);
        }
    }
}