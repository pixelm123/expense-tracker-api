using ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;
using ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;
using ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;
using ExpenseTracker.Application.Features.Expenses.Queries.GetExpenseById;
using ExpenseTracker.Application.Features.Expenses.Queries.GetExpenses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

public record UpdateExpenseRequest(decimal Amount, string Description, DateTime Date, Guid CategoryId);

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly ISender _sender;

    public ExpensesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetExpensesQuery(from, to, categoryId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = nameof(GetExpenseById))]
    public async Task<IActionResult> GetExpenseById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetExpenseByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtRoute(nameof(GetExpenseById), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateExpense(Guid id, [FromBody] UpdateExpenseRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateExpenseCommand(id, request.Amount, request.Description, request.Date, request.CategoryId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteExpenseCommand(id), cancellationToken);
        return NoContent();
    }
}
