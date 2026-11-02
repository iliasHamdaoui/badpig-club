using FluentValidation.Results;

namespace BadPig.Shared.Core;

/// <summary>
/// Outcome of a use case that returns a value: the value, or one or several <see cref="Error"/>.
/// Return the value, an <see cref="Error"/>, a list of errors or the failures of a FluentValidation result directly:
/// they convert implicitly.
/// </summary>
public class Result<TValue>
{
    protected Result(bool isSuccess, TValue value)
    {
        IsSuccess = isSuccess;
        Value = value;
    }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Errors.Add(error);
        Value = default!;
    }

    protected Result(bool isSuccess, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = [.. errors];
        Value = default!;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    /// <summary>The value; only meaningful when <see cref="IsSuccess"/>.</summary>
    public TValue Value { get; }

    /// <summary>The errors; empty when <see cref="IsSuccess"/>.</summary>
    public List<Error> Errors { get; } = [];

    public static Result<TValue> Success(TValue value) => new(true, value);

    public static Result<TValue> Failure(Error error) => new(false, error);

    public static Result<TValue> Failure(IEnumerable<Error> errors) => new(false, errors);

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure(error);

    public static implicit operator Result<TValue>(List<Error> errors) => Failure(errors);

    public static implicit operator Result<TValue>(Error[] errors) => Failure(errors);

    /// <summary>The invalid fields of a FluentValidation result (<c>validation.Errors</c>) become validation errors.</summary>
    public static implicit operator Result<TValue>(List<ValidationFailure> errors) => Failure(ValidationErrors.From(errors));

    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<List<Error>, TResult> onError) =>
        IsSuccess ? onSuccess(Value) : onError(Errors);
}

/// <summary>Outcome of a use case that returns nothing (HTTP 204 when it succeeds): <c>Result.Success()</c>, or the errors.</summary>
public class Result : Result<bool>
{
    private Result(bool isSuccess, bool value) : base(isSuccess, value) { }

    private Result(bool isSuccess, Error error) : base(isSuccess, error) { }

    private Result(bool isSuccess, IEnumerable<Error> errors) : base(isSuccess, errors) { }

    public static Result Success() => new(true, true);

    public static new Result Success(bool value) => new(true, value);

    public static new Result Failure(Error error) => new(false, error);

    public static new Result Failure(IEnumerable<Error> errors) => new(false, errors);

    public static implicit operator Result(bool value) => Success(value);

    public static implicit operator Result(Error error) => Failure(error);

    public static implicit operator Result(List<Error> errors) => Failure(errors);

    public static implicit operator Result(Error[] errors) => Failure(errors);

    public static implicit operator Result(List<ValidationFailure> errors) => Failure(ValidationErrors.From(errors));
}

internal static class ValidationErrors
{
    /// <summary>One validation error per failure: title = field in camelCase (as in the JSON, "Body.Name" gives "body.name"), detail = message.</summary>
    public static IEnumerable<Error> From(IEnumerable<ValidationFailure> failures) =>
        failures.Select(f => Error.Validation(CamelCase(f.PropertyName), f.ErrorMessage));

    private static string CamelCase(string property) =>
        string.Join('.', property.Split('.').Select(p => p.Length == 0 ? p : char.ToLowerInvariant(p[0]) + p[1..]));
}
