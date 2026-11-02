namespace BadPig.Shared.Core;

/// <summary>Kind of error; it decides the HTTP status (see <see cref="ResultExtensions"/>).</summary>
public enum ErrorType
{
    /// <summary>Generic failure (HTTP 500).</summary>
    Failure,

    /// <summary>Unexpected error, or a missing server configuration (HTTP 500).</summary>
    Unexpected,

    /// <summary>An invalid value: the title is the field, the detail the message (HTTP 400).</summary>
    Validation,

    /// <summary>The request conflicts with the current state (HTTP 409).</summary>
    Conflict,

    /// <summary>The resource does not exist (HTTP 404).</summary>
    NotFound,

    /// <summary>The person is not signed in (HTTP 401).</summary>
    Unauthorized,

    /// <summary>The person is signed in but not allowed to do this (HTTP 403).</summary>
    Forbidden,

    /// <summary>An external service (identity provider, mail…) could not be reached or refused (HTTP 502).</summary>
    BadGateway,
}
