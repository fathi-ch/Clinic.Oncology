using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Clinic.Core.Services;

// The clinic's letterhead, drawn as text and vectors so it prints sharp at any size.
// It replaces the scanned header picture formerly kept in Helpers/Images.cs; the details below were transcribed from it.
public static class ClinicLetterhead
{
    private const string Practice = "Cabinet d'oncologie médicale";
    private const string Doctor = "Dr Amrane L., oncologue médicale";
    private const string DoctorArabic = "الدكتورة عمران ل.";
    private const string SpecialtyArabic = "أخصائية طب الأورام والسرطان";
    private const string Address = "Cité Chikh Larbi Tebessi, lot 130 n°1, Tébessa";
    private const string Phone = "Tél. 0 790 104 204";

    public const string Heading = "#402060"; // Radix purple-12, the website's heading color
    public const string Accent = "#8E4EC6";  // Radix purple-9, the website's accent
    public const string Muted = "#65636D";   // Radix mauve-11

    // Same mark as the website's assets/image/logo.svg.
    private const string Logo = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 48 48">
          <rect width="48" height="48" rx="12" fill="#6E3A8C"/>
          <rect x="15" y="12" width="6" height="24" rx="1.5" fill="#FFFFFF"/>
          <path d="M21 12 A12 12 0 0 1 21 36 A7.5 12 0 0 0 21 12 Z" fill="#E2C8F0"/>
        </svg>
        """;

    public static void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(38).Height(38).Svg(Logo).FitArea();
                row.ConstantItem(10);

                row.RelativeItem().AlignMiddle().Column(left =>
                {
                    left.Item().Text(Practice).FontFamily("Exo").Bold().FontSize(17).FontColor(Heading);
                    left.Item().Text(Doctor).FontSize(10.5f).FontColor(Muted);
                });

                row.RelativeItem().AlignMiddle().ContentFromRightToLeft().Column(right =>
                {
                    right.Item().Text(DoctorArabic).FontFamily("Noto Naskh Arabic").SemiBold().FontSize(14).FontColor(Heading);
                    right.Item().Text(SpecialtyArabic).FontFamily("Noto Naskh Arabic").FontSize(10.5f).FontColor(Muted);
                });
            });

            column.Item().PaddingTop(8).Height(2).Background(Accent);

            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text(Address).FontSize(8.5f).FontColor(Muted);
                row.AutoItem().Text(Phone).FontSize(8.5f).FontColor(Muted);
            });
        });
    }
}
