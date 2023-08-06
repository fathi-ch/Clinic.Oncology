using System.Data;
using Microsoft.Data.Sqlite;

namespace Clinic.Core.Data;

public class SqliteDbConnectionFactory : ISqliteDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteDbConnectionFactory()
    {
        _connectionString = @"Data Source=D:/Code/Data/database.db";
    }

    public async Task<IDbConnection> CreateDbConnectionAsync()
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}