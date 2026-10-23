using Clinic.Core.Contracts;

namespace Clinic.Core.Services;

// Income figures for the statistics page, computed from the price and billing type of each visit.
public static class IncomeReport
{
    private const int MaxDailyBuckets = 92; // longer periods are charted by month only

    // `toLocal` turns a stored UTC time into clinic time, so a visit counts on the day it happened there.
    public static IncomeReportResponse Build(IEnumerable<VisitResponse> visits, DateOnly from, DateOnly to,
        DateTime nowLocal, Func<DateTime, DateTime> toLocal)
    {
        var dated = visits.Select(v => (Visit: v, Day: DateOnly.FromDateTime(toLocal(v.StartTime)))).ToList();
        var inPeriod = dated.Where(d => d.Day >= from && d.Day <= to).ToList();
        var today = DateOnly.FromDateTime(nowLocal);

        var report = new IncomeReportResponse { From = from, To = to };
        foreach (var (visit, _) in inPeriod) Add(report.Period, visit);

        report.Today = IncomeOf(dated.Where(d => d.Day == today));
        report.ThisMonth = IncomeOf(dated.Where(d => d.Day.Year == today.Year && d.Day.Month == today.Month));
        report.ThisYear = IncomeOf(dated.Where(d => d.Day.Year == today.Year));

        if (to.DayNumber - from.DayNumber < MaxDailyBuckets)
            report.ByDay = Buckets(inPeriod, EachDay(from, to), d => d.ToString("yyyy-MM-dd"));
        report.ByMonth = Buckets(inPeriod, EachMonth(from, to), d => d.ToString("yyyy-MM"));
        report.ByYear = Buckets(inPeriod, Enumerable.Range(from.Year, to.Year - from.Year + 1).Select(y => new DateOnly(y, 1, 1)), d => d.ToString("yyyy"));

        report.ByPatient = inPeriod
            .GroupBy(d => d.Visit.PatientId)
            .Select(g =>
            {
                var patient = g.First().Visit.Patient;
                var row = new PatientIncome
                {
                    PatientId = g.Key,
                    Name = $"{patient?.FirstName} {patient?.LastName}".Trim(),
                };
                foreach (var (visit, _) in g) Add(row, visit);
                return row;
            })
            .OrderByDescending(p => p.Income)
            .ThenBy(p => p.Name)
            .ToList();

        return report;
    }

    private static void Add(IncomeTotals totals, VisitResponse visit)
    {
        var billing = string.IsNullOrEmpty(visit.BillingType) ? "PAID" : visit.BillingType;
        var price = Math.Max(0, visit.Price);

        if (visit.Status != "TRM")
        {
            if (billing != "FREE") totals.Planned += price;
            return;
        }

        totals.FinishedVisits++;
        switch (billing)
        {
            case "FREE": totals.FreeVisits++; break;
            case "PROMO": totals.Promo += price; break;
            default: totals.Paid += price; break;
        }
    }

    private static float IncomeOf(IEnumerable<(VisitResponse Visit, DateOnly Day)> visits)
    {
        var totals = new IncomeTotals();
        foreach (var (visit, _) in visits) Add(totals, visit);
        return totals.Income;
    }

    // One bucket per key in `starts`, empty ones included, so the chart has no gaps.
    private static List<IncomeBucket> Buckets(List<(VisitResponse Visit, DateOnly Day)> visits,
        IEnumerable<DateOnly> starts, Func<DateOnly, string> key)
    {
        var buckets = starts.Select(s => new IncomeBucket { Key = key(s) }).ToList();
        var byKey = buckets.ToDictionary(b => b.Key);
        foreach (var (visit, day) in visits)
            if (byKey.TryGetValue(key(day), out var bucket)) Add(bucket, visit);
        return buckets;
    }

    private static IEnumerable<DateOnly> EachDay(DateOnly from, DateOnly to)
    {
        for (var d = from; d <= to; d = d.AddDays(1)) yield return d;
    }

    private static IEnumerable<DateOnly> EachMonth(DateOnly from, DateOnly to)
    {
        for (var m = new DateOnly(from.Year, from.Month, 1); m <= to; m = m.AddMonths(1)) yield return m;
    }
}
