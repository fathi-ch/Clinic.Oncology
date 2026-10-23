using Clinic.Core.Contracts;
using Clinic.Core.Services;

namespace Clinic.Core.Tests;

public class IncomeReportTests
{
    // The clinic runs on UTC+1 (Algeria, no daylight saving).
    private static DateTime ToLocal(DateTime utc) => utc.AddHours(1);

    private static readonly PatientResponse Amina = new() { Id = 1, FirstName = "Amina", LastName = "B" };
    private static readonly PatientResponse Karim = new() { Id = 2, FirstName = "Karim", LastName = "D" };

    private static VisitResponse Visit(PatientResponse patient, DateTime startUtc, float price, string billing = "PAID", string status = "TRM") => new()
    {
        PatientId = patient.Id,
        Patient = patient,
        StartTime = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc),
        EndTime = DateTime.SpecifyKind(startUtc.AddMinutes(15), DateTimeKind.Utc),
        Price = price,
        BillingType = billing,
        Status = status,
    };

    private static readonly DateTime Now = new(2026, 10, 24, 10, 0, 0); // local

    [Fact]
    public void Income_counts_finished_paid_and_promoted_visits_only()
    {
        var visits = new[]
        {
            Visit(Amina, new DateTime(2026, 10, 5, 8, 0, 0), 2000),
            Visit(Amina, new DateTime(2026, 10, 6, 8, 0, 0), 1200, "PROMO"),
            Visit(Karim, new DateTime(2026, 10, 7, 8, 0, 0), 0, "FREE"),
            Visit(Karim, new DateTime(2026, 10, 28, 8, 0, 0), 2500, status: "PLA"),
        };

        var report = IncomeReport.Build(visits, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), Now, ToLocal);

        Assert.Equal(2000, report.Period.Paid);
        Assert.Equal(1200, report.Period.Promo);
        Assert.Equal(3200, report.Period.Income);
        Assert.Equal(1, report.Period.FreeVisits);
        Assert.Equal(3, report.Period.FinishedVisits);
        Assert.Equal(2500, report.Period.Planned);
    }

    [Fact]
    public void A_visit_late_in_the_evening_counts_on_its_local_day()
    {
        // 23:30 UTC on the 9th is 00:30 on the 10th in the clinic
        var visits = new[] { Visit(Amina, new DateTime(2026, 10, 9, 23, 30, 0), 1000) };

        var report = IncomeReport.Build(visits, new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 10), Now, ToLocal);

        Assert.Equal(1000, report.Period.Income);
        Assert.Equal("2026-10-10", Assert.Single(report.ByDay).Key);
    }

    [Fact]
    public void Visits_outside_the_period_are_left_out_of_the_period_but_not_of_the_year()
    {
        var visits = new[]
        {
            Visit(Amina, new DateTime(2026, 9, 30, 8, 0, 0), 500),
            Visit(Amina, new DateTime(2026, 10, 2, 8, 0, 0), 700),
        };

        var report = IncomeReport.Build(visits, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), Now, ToLocal);

        Assert.Equal(700, report.Period.Income);
        Assert.Equal(700, report.ThisMonth);
        Assert.Equal(1200, report.ThisYear);
    }

    [Fact]
    public void Every_day_and_month_of_the_period_has_a_bucket_for_the_chart()
    {
        var report = IncomeReport.Build(Array.Empty<VisitResponse>(), new DateOnly(2026, 10, 1), new DateOnly(2026, 11, 15), Now, ToLocal);

        Assert.Equal(46, report.ByDay.Count);
        Assert.Equal(new[] { "2026-10", "2026-11" }, report.ByMonth.Select(m => m.Key));
        Assert.Equal(new[] { "2026" }, report.ByYear.Select(y => y.Key));
        Assert.Equal(0, report.Period.Income);
    }

    [Fact]
    public void Patients_are_ranked_by_income_with_their_visit_counts()
    {
        var visits = new[]
        {
            Visit(Amina, new DateTime(2026, 10, 5, 8, 0, 0), 1000),
            Visit(Karim, new DateTime(2026, 10, 6, 8, 0, 0), 3000),
            Visit(Karim, new DateTime(2026, 10, 7, 8, 0, 0), 0, "FREE"),
        };

        var report = IncomeReport.Build(visits, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), Now, ToLocal);

        Assert.Equal(new[] { "Karim D", "Amina B" }, report.ByPatient.Select(p => p.Name));
        var karim = report.ByPatient[0];
        Assert.Equal(3000, karim.Income);
        Assert.Equal(2, karim.FinishedVisits);
        Assert.Equal(1, karim.FreeVisits);
    }
}
