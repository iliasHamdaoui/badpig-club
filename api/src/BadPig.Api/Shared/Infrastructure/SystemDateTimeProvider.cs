using BadPig.Shared.Core;

namespace BadPig.Shared.Infrastructure;

/// <summary><see cref="IDateTimeProvider"/> backed by the system clock.</summary>
internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
