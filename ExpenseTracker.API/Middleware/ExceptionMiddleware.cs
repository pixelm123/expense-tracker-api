using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var problem = exception switch
        {
            ValidationException ve => BuildValidationProblem(ve, context),
            DomainException de => Problem(400, "Domain Rule Violation", de.Message, context),
            NotFoundException ne => Problem(404, "Not Found", ne.Message, context),
            ForbiddenException => Problem(403, "Forbidden", "You do not have permission to perform this action.", context),
            _ => ServerError(exception, context),
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem);
    }

    private static ProblemDetails BuildValidationProblem(ValidationException ve, HttpContext context)
    {
        var errors = ve.Errors
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => (object)g.Select(f => f.ErrorMessage).ToArray());

        var problem = Problem(400, "Validation Failed", "One or more validation errors occurred.", context);
        problem.Extensions["errors"] = errors;
        return problem;
    }

    private ProblemDetails ServerError(Exception exception, HttpContext context)
    {
        _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
            context.Request.Method, context.Request.Path);

        return Problem(500, "Server Error", "An unexpected error occurred. Please try again later.", context);
    }

    private static ProblemDetails Problem(int status, string title, string detail, HttpContext context)
        => new()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
        };
}
