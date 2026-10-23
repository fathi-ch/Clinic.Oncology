namespace Clinic.Core.Contracts;

// Amounts in DA. Income = finished visits billed PAID or PROMO; planned visits only appear in Planned.
public class IncomeTotals
{
    public float Paid { get; set; }
    public float Promo { get; set; }
    public float Income => Paid + Promo;
    public float Planned { get; set; }
    public int FinishedVisits { get; set; }
    public int FreeVisits { get; set; }
}

public class IncomeBucket : IncomeTotals
{
    public string Key { get; set; } = ""; // "2026-10-24", "2026-10" or "2026"
}

public class PatientIncome : IncomeTotals
{
    public int PatientId { get; set; }
    public string Name { get; set; } = "";
}

public class IncomeReportResponse
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public IncomeTotals Period { get; set; } = new();
    public float Today { get; set; }
    public float ThisMonth { get; set; }
    public float ThisYear { get; set; }
    public List<IncomeBucket> ByDay { get; set; } = new();
    public List<IncomeBucket> ByMonth { get; set; } = new();
    public List<IncomeBucket> ByYear { get; set; } = new();
    public List<PatientIncome> ByPatient { get; set; } = new();
}
