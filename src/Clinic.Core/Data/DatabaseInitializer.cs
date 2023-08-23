using Dapper;

namespace Clinic.Core.Data;

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
                                      " Id INTEGER PRIMARY KEY," +
                                      " FirstName TEXT NOT NULL," +
                                      " LastName TEXT NOT NULL," +
                                      " BirthDate DATE DEFAULT CURRENT_DATE NOT NULL," +
                                      " NextAppointment DATE," +
                                      " Weight REAL," +
                                      " Height REAL," +
                                      " Gender TEXT NOT NULL," +
                                      " Mobile TEXT NOT NULL," +
                                      " SocialSecurityNumber TEXT, " +
                                      " Referral TEXT); " +
                                      "CREATE TABLE IF NOT EXISTS Visits (" +
                                      " Id INTEGER PRIMARY KEY," +
                                      " PatientId INTEGER NOT NULL," +
                                      " StartTime Date NOT NULL," +
                                      " EndTime Date NOT NULL," +
                                      " Price REAL," +
                                      " Description TEXT," +
                                      " FOREIGN KEY(PatientId) REFERENCES Patients(Id));");
        
    }
}

