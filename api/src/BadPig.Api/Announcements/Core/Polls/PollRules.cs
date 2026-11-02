using BadPig.Shared.Core;

namespace BadPig.Announcements.Core;

/// <summary>Rules shared by the features that read or change a poll (dates are Europe/Paris dates).</summary>
public static class PollRules
{
    public const int MinOptions = 2;
    public const int MaxOptions = 10;
    public const int MaxLabelLength = 100;

    /// <summary>A poll is closed the day after its closing date: the closing day itself, answers are still possible.</summary>
    public static bool IsClosed(DateOnly closesOn, DateOnly today) => closesOn < today;

    /// <summary>The error to return when the chosen closing date is missing or already past; null when it is acceptable.</summary>
    public static Error? CheckClosingDate(DateOnly? closesOn, DateOnly today)
    {
        if (closesOn is not { } date)
            return Error.Validation("closesOn", "La date limite du sondage est obligatoire.");

        return IsClosed(date, today) ? Error.Validation("closesOn", "La date limite du sondage ne peut pas être dans le passé.") : null;
    }

    public static Error ClosedError() => Error.Conflict("Le sondage est clos : les réponses ne peuvent plus être modifiées.");
}
