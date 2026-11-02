using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shared.Core;

/// <summary>Turns the outcome of a handler into the HTTP answer (used by the endpoint classes).</summary>
public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : result.Errors.ToActionResult();

    /// <summary>200 with the value as JSON.</summary>
    public static IActionResult ToOk<T>(this Result<T> result) => result.ToActionResult(value => new OkObjectResult(value));

    /// <summary>201 with the identifier of the created resource as JSON (a creation returns the identifier only).</summary>
    public static IActionResult ToCreated<T>(this Result<T> result) =>
        result.ToActionResult(value => new ObjectResult(value) { StatusCode = StatusCodes.Status201Created });

    /// <summary>204 without content.</summary>
    public static IActionResult ToNoContent(this Result result) => result.ToActionResult(_ => new NoContentResult());

    /// <summary>
    /// The HTTP answer of failed use case. Validation errors (any number) give one 400 listing every invalid field;
    /// otherwise the first error decides: 401, 403, 404 (no body), 409, 502, or 500.
    /// </summary>
    public static IActionResult ToActionResult(this IReadOnlyList<Error> errors)
    {
        var validation = errors.Where(e => e.Type == ErrorType.Validation).ToList();
        if (validation.Count > 0)
            return ApiResults.Validation(validation.GroupBy(e => e.Title).ToDictionary(g => g.Key, g => g.Select(e => e.Detail).Distinct().ToArray()));

        var error = errors.Count > 0 ? errors[0] : Error.Unexpected();
        return error.Type switch
        {
            ErrorType.NotFound => new NotFoundResult(),
            ErrorType.Unauthorized => ApiResults.Problem(StatusCodes.Status401Unauthorized, error),
            ErrorType.Forbidden => ApiResults.Problem(StatusCodes.Status403Forbidden, error),
            ErrorType.Conflict => ApiResults.Problem(StatusCodes.Status409Conflict, error),
            ErrorType.BadGateway => ApiResults.Problem(StatusCodes.Status502BadGateway, error),
            _ => ApiResults.Problem(StatusCodes.Status500InternalServerError, error),
        };
    }
}
