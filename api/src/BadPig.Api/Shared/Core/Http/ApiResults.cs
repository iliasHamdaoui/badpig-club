using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shared.Core;

/// <summary>Standard error answers of the API (RFC 9457 problem details). The "title" is shown to the person: it is in French.</summary>
public static class ApiResults
{
    public static ObjectResult Problem(int statusCode, string title) =>
        new(new ProblemDetails { Status = statusCode, Title = title }) { StatusCode = statusCode };

    /// <summary>The title is the message shown to the person; the detail is only sent when the error has one.</summary>
    public static ObjectResult Problem(int statusCode, Error error) =>
        new(new ProblemDetails { Status = statusCode, Title = error.Title, Detail = error.Detail is { Length: > 0 } ? error.Detail : null }) { StatusCode = statusCode };

    public static ObjectResult Validation(string field, string message) =>
        Validation(new Dictionary<string, string[]> { [field] = [message] });

    public static ObjectResult Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(new ValidationProblemDetails(errors.ToDictionary(e => e.Key, e => e.Value)) { Status = StatusCodes.Status400BadRequest })
        {
            StatusCode = StatusCodes.Status400BadRequest,
        };
}
