public interface IPatientReportRepository
{
    Task<byte[]> GeneratePatientReportPdfAsync(int id);
}