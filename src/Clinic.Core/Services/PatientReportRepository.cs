
using Clinic.Core.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

public class PatientReportRepository : IPatientReportRepository
{
    private readonly IPatientRepository _patientRepository;
    private readonly IDocumentRepository _documentRepository;

    public PatientReportRepository(IPatientRepository patientRepository, IDocumentRepository documentRepository)
    {
        _patientRepository = patientRepository;
        _documentRepository = documentRepository;
    }

    public async Task<byte[]> GeneratePatientReportPdfAsync(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null)
            throw new Exception("Patient not found.");

        var patientDocumentsResponse = await _documentRepository.GetByPatientIdAsync(id);
        var files = patientDocumentsResponse.Select(x => Convert.FromBase64String(x.PatientDocumentsbase64)).ToList();

        if (!files.Any())
        {
            return null;
        }

        try
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var report = Document.Create(container =>
           {

               container.Page(page =>
              {
                  page.Size(PageSizes.A4);
                  page.Margin(10, Unit.Millimetre);

                  page.Content()
                  .PaddingVertical(2, Unit.Millimetre)
                  .PaddingHorizontal(5, Unit.Millimetre)
                  .Column(x =>
                  {
                      x.Item().Image(Convert.FromBase64String(Images.HeaderBase64));

                      x.Item().Text($"Date: {DateTime.UtcNow.ToShortDateString()}").AlignRight();
                      x.Item().Row(row =>
                      {

                          row.RelativeItem().Text($"NOM: {patient.LastName}").AlignCenter();
                          row.RelativeItem().Text($"PRENOM: {patient.FirstName}").AlignCenter();
                          row.RelativeItem().Text($"AGE: {patient.Age}").AlignCenter();

                      });
                      x.Spacing(10);

                      foreach (var file in files)
                      {
                          x.Item().Image(file);
                      }

                  });

                  page.Footer()
                 .PaddingVertical(2, Unit.Millimetre)
                 .PaddingHorizontal(5, Unit.Millimetre)
                 .Column(column =>
                 {
                     column.Item().LineHorizontal(1);
                     column.Spacing(3);
                     column.Item()
                         .AlignRight()
                         .Text(text =>
                         {
                             text.Span("Page ");
                             text.CurrentPageNumber();
                         });
                 });
              });
           }).GeneratePdf();

            return report;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}