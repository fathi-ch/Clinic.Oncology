using Clinic.Core.Configurations;
using Clinic.Core.Data;
using Clinic.Core.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var dbConfig = new DatabaseConfigurations();
builder.Configuration.GetSection(DatabaseConfigurations.DatabaseConfigurationsSection).Bind(dbConfig);
builder.Services.AddSingleton(dbConfig);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ISqliteDbConnectionFactory, SqliteDbConnectionFactory>();
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<IPatientRepository, PatientRepository>();
builder.Services.AddSingleton<IPatientDocumentRepository, PatientDocumentRepository>();
builder.Services.AddSingleton<IFileRepository, FileRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var dbInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await dbInitializer.InitializeAsync();


app.Run();