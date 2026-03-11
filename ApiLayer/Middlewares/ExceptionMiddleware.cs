using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using EntityLayer.DTOs.ErrorDetails;
using EntityLayer.Exceptions;

namespace ApiLayer.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred. Path: {Path}, Method: {Method}",
                    context.Request.Path,
                    context.Request.Method);

                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode = (int)HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred on the server.";

            if (exception is BusinessException bizEx)
            {

                statusCode = (int)HttpStatusCode.BadRequest;

                string key = bizEx.ErrorKey;
                string msgFromConfig = _configuration[$"ErrorConfig:{key}:Message"];

                if (!string.IsNullOrEmpty(msgFromConfig))
                {
                    message = msgFromConfig;
                }
                else
                {
                    message = $"Error: {key}";
                }
            }


            context.Response.StatusCode = statusCode;


            var errorDetails = new ErrorDetails
            {
                Message = message
            };

            return context.Response.WriteAsync(errorDetails.ToString());
        }
    }
}