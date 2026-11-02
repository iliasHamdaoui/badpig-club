namespace BadPig.Shared.Core;

/// <summary>The current date and time. Code never reads the system clock directly: tests replace this interface.</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
