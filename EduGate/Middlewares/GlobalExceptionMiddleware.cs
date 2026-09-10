using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Application.Exceptions;
namespace WebApi.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = exception.Message;

            switch (exception)
            {
                case NotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    break;

                case ConflictException:
                    statusCode = (int)HttpStatusCode.Conflict;
                    break;

                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    break;

                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case FluentValidation.ValidationException validationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = validationException.Message;
                    break;

                case ArgumentException:
                case InvalidOperationException:
                case FormatException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case DbUpdateConcurrencyException:
                    statusCode = (int)HttpStatusCode.Conflict;
                    message = "A concurrency conflict occurred. The data you are trying to update was modified by another transaction.";
                    break;

                case NotImplementedException:
                    statusCode = (int)HttpStatusCode.NotImplemented;
                    break;

               

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    message = "An internal server error occurred.";
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                message = message,
                statusCode = statusCode
            };

            var result = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(result);
        }
    }
}