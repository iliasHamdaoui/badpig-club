using BadPig.Shared.Core;

namespace BadPig.Usage.Core;

/// <summary>The periods the board can look at (ending today, in Paris), shared by the lists and the chart of the usage statistics.</summary>
public static class UsagePeriods
{
    public static readonly int[] Allowed = [7, 30, 90, 365];
    public const int Default = 30;

    public static bool IsKnown(int? days) => days is null || Allowed.Contains(days.Value);

    /// <summary>The first day of the period, today included in its length.</summary>
    public static DateOnly Start(DateTimeOffset utcNow, int? days) => ClubTime.Today(utcNow).AddDays(1 - (days ?? Default));
}
