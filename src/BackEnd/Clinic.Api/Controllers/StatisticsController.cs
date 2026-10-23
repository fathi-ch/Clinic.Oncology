using Clinic.Api.BusinessService;
using Clinic.Core.Contracts;
using Clinic.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("/v1/api/statistics")]
public class StatisticsController : ControllerBase
{
    private const int MaxPeriodDays = 3660; // ten years

    private readonly IVisitsService _visitsService;

    public StatisticsController(IVisitsService visitsService)
    {
        _visitsService = visitsService;
    }

    // Income of finished visits between two dates (inclusive, clinic time), plus today, this month and this year.
    [HttpGet("income", Name = "GetIncomeAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncomeReportResponse>> GetIncomeAsync([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        if (to < from)
            return BadRequest(new { Message = "La date de fin doit être après la date de début." });
        if (to.DayNumber - from.DayNumber > MaxPeriodDays)
            return BadRequest(new { Message = "La période ne peut pas dépasser dix ans." });

        var visits = await _visitsService.GetAllAsync();

        // Visit times are stored in UTC; the server runs in the clinic's time zone.
        return Ok(IncomeReport.Build(visits, from, to, DateTime.Now, utc => utc.ToLocalTime()));
    }
}
