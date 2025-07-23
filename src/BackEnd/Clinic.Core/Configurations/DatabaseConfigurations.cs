using Clinic.Core.Helpers;

namespace Clinic.Core.Configurations;

public class DatabaseConfigurations
{
    public const string DatabaseConfigurationsSection = "DatabaseConfigurations";
    public string? DatabasePath { get; set; }
    public string? DatabaseConfigFilePath { get; set; }
    public string? DatabaseName { get; set; }
    public string? DocumentsPath { get; set; }

    //Temporary till this all the helper methods turned to Extension Methods
    public string GetFullSaveFolderPathForDocuments(string fileName)
    {
        Guard.IsNotNull(DatabasePath, nameof(DatabasePath));
        Guard.IsNotNull(DocumentsPath, nameof(DocumentsPath));

        int indexOfUnderscore = fileName.IndexOf('_');
        var directoryName = fileName.Substring(0, indexOfUnderscore);

        return Directory.CreateDirectory(Path.Combine(DatabasePath, DocumentsPath, directoryName)).ToString();
    }
}