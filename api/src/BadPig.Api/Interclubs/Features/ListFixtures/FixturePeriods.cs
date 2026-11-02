namespace BadPig.Interclubs.Features.ListFixtures;

/// <summary>Values of the "when" filter of the calendar.</summary>
internal static class FixturePeriods
{
    public const string Upcoming = "upcoming";
    public const string Past = "past";

    /// <summary>Being played right now (see <see cref="LiveFixtures"/>).</summary>
    public const string Live = "live";

    public static readonly string[] All = [Upcoming, Past, Live];
}
