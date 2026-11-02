namespace BadPig.Shared.Core;

/// <summary>
/// Why a use case failed. The <see cref="Title"/> is a short message shown to the person (French);
/// the optional <see cref="Detail"/> adds an explanation. For a <see cref="ErrorType.Validation"/> error the title is the
/// name of the invalid field and the detail is the message about it.
/// </summary>
public readonly record struct Error
{
    public string Title { get; }
    public string Detail { get; }
    public ErrorType Type { get; }

    private Error(string title, string detail, ErrorType type)
    {
        Title = title;
        Detail = detail;
        Type = type;
    }

    public static Error Failure(string title = "Failure", string detail = "") => new(title, detail, ErrorType.Failure);

    public static Error Unexpected(string title = "Unexpected error", string detail = "") => new(title, detail, ErrorType.Unexpected);

    /// <param name="title">Name of the invalid field (camelCase, as in the JSON).</param>
    /// <param name="detail">What is wrong with it (French, shown to the person).</param>
    public static Error Validation(string title = "Validation error", string detail = "") => new(title, detail, ErrorType.Validation);

    public static Error Conflict(string title = "Conflict", string detail = "") => new(title, detail, ErrorType.Conflict);

    public static Error NotFound(string title = "Not found", string detail = "") => new(title, detail, ErrorType.NotFound);

    public static Error Unauthorized(string title = "Unauthorized", string detail = "") => new(title, detail, ErrorType.Unauthorized);

    public static Error Forbidden(string title = "Forbidden", string detail = "") => new(title, detail, ErrorType.Forbidden);

    public static Error BadGateway(string title = "Bad gateway", string detail = "") => new(title, detail, ErrorType.BadGateway);
}
