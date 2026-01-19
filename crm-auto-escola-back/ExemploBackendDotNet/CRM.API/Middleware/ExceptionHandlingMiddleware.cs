using Exemplo.Service.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiException ex)
            {
                _logger.LogWarning(ex, "Erro tratado: {Title}", ex.Title);
                await WriteProblemDetails(context, ex.StatusCode, ex.Title, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado.");
                await WriteProblemDetails(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "Erro interno",
                    "Ocorreu um erro inesperado. Tente novamente mais tarde.");
            }
        }

        private static async Task WriteProblemDetails(
            HttpContext context,
            int statusCode,
            string title,
            string detail)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
