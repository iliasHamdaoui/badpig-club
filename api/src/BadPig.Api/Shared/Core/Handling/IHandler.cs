namespace BadPig.Shared.Core;

/// <summary>
/// Use case of a feature that returns a value: it receives the request model and returns the response model (or errors).
/// Handlers are found and registered automatically (see <c>SharedModule</c>); the endpoint class only does HTTP.
/// </summary>
public interface IHandler<in TRequest, TResponse>
{
    Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken ct);
}

/// <summary>Use case of a feature that returns nothing (HTTP 204): it succeeds with <c>Result.Success()</c> or returns errors.</summary>
public interface IHandler<in TRequest>
{
    Task<Result> HandleAsync(TRequest request, CancellationToken ct);
}
