using System.Data;
using Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.WebApi.Filters;

public class ApiExceptionFilter : ExceptionFilterAttribute
{
    private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
        // Register known exception types and handlers.
        _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(DataException), HandleValidationException },
            { typeof(LogicalException), HandleUnknownException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(UnprocessableEntityException), HandleUnprocessableEntityException },
            { typeof(FileUploadException), FileUploadDownloadException },
            { typeof(FileDownloadException), FileUploadDownloadException },
            { typeof(UnauthorizedException), HandleUnauthorizedException },
            { typeof(FeatureException), HandleFeatureException },
            { typeof(ForbiddenException), HandleForbiddenException },
            { typeof(ConflictException), HandleConflictException },
            { typeof(SystemException), HandleSystemException },
            { typeof(ParameterException), HandleParameterException },
            { typeof(UserLockedOutException), HandleUserLockOutException }
        };
    }

    public override void OnException(ExceptionContext context)
    {
        _logger.LogError(context?.Exception, "Unexpected error occurred. {traceIdentifier}",
            context?.HttpContext?.TraceIdentifier);
        HandleException(context);
        base.OnException(context);
    }

    private void HandleConflictException(ExceptionContext context)
    {
        var exception = context.Exception as ConflictException;
        var title = exception?.Message ?? "An error occurred while processing your request.";
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = title,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
        };
        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status409Conflict
        };
        context.ExceptionHandled = true;
    }

    private void HandleException(ExceptionContext context)
    {
        var type = context.Exception.GetType();
        if (_exceptionHandlers.ContainsKey(type))
        {
            _exceptionHandlers[type].Invoke(context);
            return;
        }

        HandleUnknownException(context);
    }

    private void HandleUnknownException(ExceptionContext context)
    {
        var exception = context.Exception;
        var title = exception?.Message ?? "An error occurred while processing your request.";
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = title,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = true;
    }

    private void HandleValidationException(ExceptionContext context)
    {
        var exception = context.Exception as ValidationException;
        var details = new ValidationProblemDetails(exception.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };
        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private void HandleNotFoundException(ExceptionContext context)
    {
        var exception = context.Exception as NotFoundException;
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found.",
            Detail = exception.Message
        };
        context.Result = new NotFoundObjectResult(details);
        context.ExceptionHandled = true;
    }

    private void HandleUnprocessableEntityException(ExceptionContext context)
    {
        var exception = context.Exception as UnprocessableEntityException;
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The request was well-formed but was unable to be followed due to semantic errors.",
            Detail = exception.Message
        };
        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity
        };

        context.ExceptionHandled = true;
    }

    private void FileUploadDownloadException(ExceptionContext context)
    {
        var exception = context.Exception;
        var filetype = exception is FileUploadException ? "Upload" : "Download";
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = $"File {filetype} Error",
            Detail = exception.Message
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;
    }

    private void HandleUnauthorizedException(ExceptionContext context)
    {
        var exception = context.Exception as UnauthorizedException;
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Unauthorized",
            Detail = exception.Message
        };
        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };
        context.ExceptionHandled = true;
    }

    private void HandleFeatureException(ExceptionContext context)
    {
        var exception = context.Exception as FeatureException;
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "User does not have access to this feature for this project",
            Detail = exception.Message
        };
        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status405MethodNotAllowed
        };
        context.ExceptionHandled = true;
    }

    private void HandleForbiddenException(ExceptionContext context)
    {
        var exception = context.Exception as ForbiddenException;
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Forbidden",
            Detail = exception.Message
        };
        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
        context.ExceptionHandled = true;
    }

    private void HandleSystemException(ExceptionContext context)
    {
        var exception = context.Exception as UnprocessableEntityException;
        var errorMessage = context.Exception.Message;
        if (exception != null && !string.IsNullOrWhiteSpace(exception.Message))
            errorMessage = exception.Message;
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The request was well-formed but processing logic failed.",
            Detail = errorMessage
        };
        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity
        };
        context.ExceptionHandled = true;
    }

    private void HandleParameterException(ExceptionContext context)
    {
        var exception = context.Exception as ParameterException;
        var errorMessage = context.Exception.Message;

        if (exception != null && !string.IsNullOrWhiteSpace(exception.Message))
            errorMessage = exception.Message;

        ProblemDetails details = new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Client request was missing or had invalid parameters.",
            Detail = errorMessage
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
        context.ExceptionHandled = true;
    }

    private void HandleUserLockOutException(ExceptionContext context)
    {
        var exception = context.Exception as ParameterException;
        var errorMessage = context.Exception.Message;


        if (exception != null && !string.IsNullOrWhiteSpace(exception.Message))
            errorMessage = exception.Message;

        ProblemDetails details = new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Exceeded Login Attempts",
            Detail = errorMessage
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status429TooManyRequests
        };
        context.ExceptionHandled = true;
    }
}