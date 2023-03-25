using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Core.Data
{
    public class DatabaseInitializer
    {
        private readonly ISqliteDbConnectionFactory _connectionFactory;

        public DatabaseInitializer(ISqliteDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InitializeAsync()
        {
            SqlMapper.AddTypeHandler(new SqLiteGuidTypeHandler());
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(Guid?));

            using var connection = await _connectionFactory.CreateDbConnectionAsync();
            await connection.ExecuteAsync("CREATE TABLE IF NOT EXISTS Patients (" +
                "Id TEXT PRIMARY KEY," +
                " FirstName TEXT NOT NULL," +
                " LastName TEXT NOT NULL," +
                " BirthDate DATE DEFAULT CURRENT_DATE NOT NULL);" +
                "CREATE TABLE IF NOT EXISTS Documents (" +
                "Id TEXT PRIMARY KEY," +
                "PatientId TEXT NOT NULL," +
                "Path TEXT NOT NULL," +
                "FOREIGN KEY(PatientId) REFERENCES Patients(Id));");

        }
        

    }
}
