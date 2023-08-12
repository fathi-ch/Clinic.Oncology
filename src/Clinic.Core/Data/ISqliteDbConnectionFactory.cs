using System.Data;

namespace Clinic.Core.Data;

public interface ISqliteDbConnectionFactory
{
    Task<IDbConnection> CreateDbConnectionAsync();
}