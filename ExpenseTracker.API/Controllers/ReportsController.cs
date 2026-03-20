using ExpenseTracker.Application.Features.Reports.Queries.GetMonthlySummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender) => _sender = sender;

    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int month, [FromQuery] int year, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMonthlySummaryQuery(month, year), cancellationToken);
        return Ok(result);
    }
}
