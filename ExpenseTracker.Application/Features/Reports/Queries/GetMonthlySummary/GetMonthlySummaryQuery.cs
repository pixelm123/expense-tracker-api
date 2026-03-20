using ExpenseTracker.Application.DTOs;
using MediatR;

namespace ExpenseTracker.Application.Features.Reports.Queries.GetMonthlySummary;

public record GetMonthlySummaryQuery(int Month, int Year) : IRequest<MonthlySummaryDto>;
