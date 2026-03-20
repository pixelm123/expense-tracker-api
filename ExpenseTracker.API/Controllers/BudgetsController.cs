using ExpenseTracker.Application.Features.Budgets.Commands.CreateBudget;
using ExpenseTracker.Application.Features.Budgets.Commands.DeleteBudget;
using ExpenseTracker.Application.Features.Budgets.Commands.UpdateBudget;
using ExpenseTracker.Application.Features.Budgets.Queries.GetBudgets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

public record UpdateBudgetRequest(decimal LimitAmount, decimal AlertThresholdPercentage);

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly ISender _sender;

    public BudgetsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetBudgets(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBudgetsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetBudgets), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBudget(Guid id, [FromBody] UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateBudgetCommand(id, request.LimitAmount, request.AlertThresholdPercentage), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBudget(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteBudgetCommand(id), cancellationToken);
        return NoContent();
    }
}
