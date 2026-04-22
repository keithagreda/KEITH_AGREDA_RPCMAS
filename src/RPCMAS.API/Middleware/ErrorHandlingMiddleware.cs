using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RPCMAS.Core.Exceptions;

namespace RPCMAS.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (ValidationException ex)
        {
            await WriteAsync(ctx, StatusCodes.Status400BadRequest, "Validation failed",
                ex.Errors.Select(e => new { Field = e.PropertyName, e.ErrorMessage }));
        }
        catch (NotFoundException ex)
        {
            await WriteAsync(ctx, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            await WriteAsync(ctx, StatusCodes.Status422UnprocessableEntity, ex.Message, code: ex.Code);
        }
        catch (ConcurrencyException ex)
        {
            await WriteAsync(ctx, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteAsync(ctx, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteAsync(HttpContext ctx, int status, string detail, object? errors = null, string? code = null)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = ReasonPhrase(status),
            Detail = detail
        };
        if (errors is not null) problem.Extensions["errors"] = errors;
        if (code is not null) problem.Extensions["code"] = code;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return ctx.Response.WriteAsync(json);
    }

    private static string ReasonPhrase(int status) => status switch
    {
        400 => "Bad Request",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        _ => "Server Error"
    };
}
