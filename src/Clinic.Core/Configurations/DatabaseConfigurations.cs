using Clinic.Core.Helpers;

namespace Clinic.Core.Configurations;

public class DatabaseConfigurations
{
    public const string DatabaseConfigurationsSection = "DatabaseConfigurations"; 
    public string? DatabasePath { get; set; }
    public string? DatabaseName { get; set; }
    public string? DocumentsPath { get; set; }

    public string GetFullDocumentsPath()
    {
        Guard.IsNotNull(DatabasePath, DocumentsPath);
        return Path.Combine(DatabasePath, DocumentsPath);
    }
}