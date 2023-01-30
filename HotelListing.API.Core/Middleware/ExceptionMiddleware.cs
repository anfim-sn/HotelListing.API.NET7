using System.Net;
using HotelListing.API.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HotelListing.API.Core.Middleware;

public class ExceptionMiddleware
{
  private readonly ILogger<ExceptionMiddleware> _logger;
  private readonly RequestDelegate next;

  public ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger
  )
  {
    this.next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await next(context);
    }
    catch (Exception e)
    {
      _logger.LogError(
        e,
        $"Something went wrong wile processing {context.Request.Path}"
      );
      _logger.LogError($"{e}");
      await HandleExceptionAsync(context, e);
    }
  }

  private Task HandleExceptionAsync(HttpContext context, Exception e)
  {
    context.Response.ContentType = "application/json";
    var statusCode = HttpStatusCode.InternalServerError;
    var errorDetails = new ErrorDetails
    {
      ErrorType = "Failure", ErrorMessage = e.Message
    };

    switch (e)
    {
      case NotFoundException notFoundException:
        statusCode = HttpStatusCode.NotFound;
        errorDetails.ErrorType = "Not Found";
        break;
    }

    var response = JsonConvert.SerializeObject(errorDetails);

    context.Response.StatusCode = (int)statusCode;

    return context.Response.WriteAsync(response);
  }
}

public class ErrorDetails
{
  public string ErrorType { get; set; }
  public string ErrorMessage { get; set; }
}
