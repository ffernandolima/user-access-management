using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UserAccessManagement.Application.Exceptions;

namespace UserAccessManagement.Middlewares
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new System.ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ConflictException exception)
            {
                await HandleConflictException(context, exception);
            }
            catch (ValidationException exception)
            {
                await HandleValidationException(context, exception);
            }
            catch (Exception exception)
            {
                await HandleException(context, exception);
            }
        }

        private static async Task HandleConflictException(HttpContext context, ConflictException exception)
        {
            var exceptionType = exception.GetType();

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-409-conflict",
                Title = exceptionType.FullName,
                Detail = exception.Message
            };

            context.Response.StatusCode = StatusCodes.Status409Conflict;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        private static async Task HandleValidationException(HttpContext context, ValidationException exception)
        {
            var exceptionType = exception.GetType();

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
                Title = exceptionType.FullName,
                Detail = "One or more validation errors has occurred."
            };

            if (exception.Errors is not null)
            {
                problemDetails.Extensions["errors"] = exception.Errors;
            }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        private static async Task HandleException(HttpContext context, Exception exception)
        {
            var exceptionType = exception.GetType();

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Title = exceptionType.FullName,
                Detail = exception.Message
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
