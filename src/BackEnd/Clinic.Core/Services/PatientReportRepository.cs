using System.Globalization;
using Clinic.Core.Contracts;
using Clinic.Core.Services;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// The patient file: patient details, then every visit in date order with its notes and documents.
public class PatientReportRepository : IPatientReportRepository
{
    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");
    private const string Heading = ClinicLetterhead.Heading;
    private const string Muted = ClinicLetterhead.Muted;
    private const string Line = "#DBD8E0"; // Radix mauve-6

    // Exo, Roboto and Noto Naskh Arabic ship inside Clinic.Core, so the PDF looks the same on any server.
    static PatientReportRepository()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var assembly = typeof(PatientReportRepository).Assembly;
        foreach (var name in assembly.GetManifestResourceNames().Where(n => n.EndsWith(".ttf")))
            FontManager.RegisterFont(assembly.GetManifestResourceStream(name)!);
    }

    private readonly IPatientRepository _patientRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IVisitRepository _visitRepository;

    public PatientReportRepository(IPatientRepository patientRepository, IDocumentRepository documentRepository,
        IVisitRepository visitRepository)
    {
        _patientRepository = patientRepository;
        _documentRepository = documentRepository;
        _visitRepository = visitRepository;
    }

    // Null when the patient does not exist.
    public async Task<byte[]?> GeneratePatientReportPdfAsync(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null) return null;

        var visits = (await _visitRepository.GetAllAsync())
            .Where(v => v.PatientId == id)
            .OrderBy(v => v.StartTime)
            .ToList();
        var documentsByVisit = (await _documentRepository.GetByPatientIdAsync(id))
            .GroupBy(d => d.VisitId)
            .ToDictionary(g => g.Key, g => g.OrderBy(d => d.Name).ToList());

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(12, Unit.Millimetre);
                // Arabic as a fallback, for patient names written in Arabic
                page.DefaultTextStyle(t => t.FontSize(10).FontFamily("Roboto", "Noto Naskh Arabic"));

                page.Header().Column(header =>
                {
                    header.Item().Element(ClinicLetterhead.Compose);
                    header.Item().PaddingTop(14).Row(row =>
                    {
                        row.RelativeItem().Text($"{patient.FirstName} {patient.LastName}").FontFamily("Exo").Bold().FontSize(14).FontColor(Heading);
                        row.AutoItem().AlignBottom().Text($"Dossier exporté le {DateTime.Now.ToString("dd/MM/yyyy", French)}").FontColor(Muted);
                    });
                    header.Item().PaddingTop(4).LineHorizontal(1).LineColor(Line);
                });

                page.Content().PaddingTop(10).Column(content =>
                {
                    content.Spacing(14);
                    content.Item().Element(c => PatientDetails(c, patient));

                    if (visits.Count == 0)
                        content.Item().Text("Aucune visite enregistrée pour ce patient.").FontColor(Muted);

                    foreach (var visit in visits)
                    {
                        var documents = documentsByVisit.TryGetValue(visit.Id, out var docs) ? docs : new List<PatientDocumentResponse>();
                        content.Item().Element(c => VisitSection(c, visit, documents));
                    }
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(t => t.FontColor(Muted));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    // The five essential fields on one row, label above value like the website's "Informations" card.
    // Email, social security number and referral follow on a second line only when they are filled in.
    private static void PatientDetails(IContainer container, PatientResponse patient)
    {
        var age = patient.Age == 1 ? "1 an" : $"{patient.Age} ans";
        // widths: the date with the age needs the most room, the sex the least
        var essentials = new (string Label, string? Value, float Width)[]
        {
            ("Nom", patient.FirstName, 1),
            ("Prénom", patient.LastName, 1),
            ("Sexe", patient.Gender, 0.6f),
            ("Date de naissance", $"{patient.BirthDate.ToString("dd/MM/yyyy", French)} ({age})", 1.5f),
            ("Mobile", patient.Mobile, 1),
        };
        var optional = new (string Label, string? Value, float Width)[]
        {
            ("Email", patient.Email, 1),
            ("N° sécurité sociale", patient.SocialSecurityNumber, 1),
            ("Référence", patient.Referral, 1),
        }.Where(f => !string.IsNullOrWhiteSpace(f.Value)).ToList();

        container.Background("#FAF9FB").Border(1).BorderColor(Line).Padding(12).Column(column =>
        {
            column.Spacing(10);
            column.Item().Text("Informations du patient").FontFamily("Exo").Bold().FontSize(11).FontColor(Heading);
            column.Item().Element(c => FieldRow(c, essentials, 11));
            if (optional.Count > 0)
                column.Item().BorderTop(1).BorderColor(Line).PaddingTop(8).Element(c => FieldRow(c, optional, 9.5f));
        });
    }

    private static void FieldRow(IContainer container, IEnumerable<(string Label, string? Value, float Width)> fields, float valueSize)
    {
        container.Row(row =>
        {
            row.Spacing(12);
            foreach (var (label, value, width) in fields)
            {
                row.RelativeItem(width).Column(field =>
                {
                    field.Item().Text(label).FontSize(8.5f).FontColor(Muted);
                    field.Item().PaddingTop(2).Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(valueSize).FontColor(Heading);
                });
            }
        });
    }

    private static void VisitSection(IContainer container, VisitResponse visit, List<PatientDocumentResponse> documents)
    {
        // Visit times are stored in UTC; the server runs in the clinic's time zone.
        var start = visit.StartTime.ToLocalTime();
        var end = visit.EndTime.ToLocalTime();
        var title = $"Visite du {start.ToString("dddd d MMMM yyyy", French)}, {start:HH:mm} – {end:HH:mm}";
        var status = visit.Status == "TRM" ? "Terminée" : "Planifiée";

        container.Column(section =>
        {
            section.Spacing(6);
            section.Item().Text(title).FontFamily("Exo").Bold().FontSize(12).FontColor(Heading);
            section.Item().Text(status).FontColor(Muted);

            if (!string.IsNullOrWhiteSpace(visit.Description))
                section.Item().Background("#FBF7FE").Padding(8).Text(visit.Description);

            if (documents.Count == 0)
            {
                section.Item().Text("Aucun document pour cette visite.").FontColor(Muted);
                return;
            }

            foreach (var document in documents)
            {
                var image = TryReadImage(document);
                if (image != null)
                    // A fixed-height frame moves a photo to the next page when it does not fit,
                    // instead of shrinking it into the space left (which hid later photos).
                    // Original resolution: QuestPDF's resize step fails natively on some images (seen with tiny PNGs).
                    section.Item().Height(360).AlignCenter().AlignMiddle().Image(image).FitArea().UseOriginalImage();
                else
                    section.Item().Text(IsPdf(document)
                        ? $"Pièce jointe PDF ({document.Name}) : à ouvrir depuis l'application."
                        : $"Document illisible ({document.Name}).").FontColor(Muted);
            }
        });
    }

    private static bool IsPdf(PatientDocumentResponse document) =>
        string.Equals(document.DocumentType, ".pdf", StringComparison.OrdinalIgnoreCase);

    // PDFs cannot be drawn as images, and a damaged file must not stop the whole export.
    private static Image? TryReadImage(PatientDocumentResponse document)
    {
        if (IsPdf(document) || string.IsNullOrEmpty(document.PatientDocumentsbase64)) return null;
        try
        {
            return Image.FromBinaryData(Convert.FromBase64String(document.PatientDocumentsbase64));
        }
        catch (Exception)
        {
            return null;
        }
    }
}
