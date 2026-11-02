namespace BadPig.Interclubs.Features.ListTeamMembers;

/// <summary>A player in the list.</summary>
/// <param name="AccountId">Account identifier of the player.</param>
/// <param name="Name">Name of the player; "Compte supprimé" if the account no longer exists.</param>
/// <param name="IsCaptain">True for the captain of the team.</param>
public sealed record ListTeamMembersItem(string AccountId, string Name, bool IsCaptain);
