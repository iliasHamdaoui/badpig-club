namespace BadPig.Administration.Features.ListBoardMembers;

/// <summary>A person of the board, as the club members see them.</summary>
/// <param name="AccountId">Account identifier of the person.</param>
/// <param name="Name">Name of the person.</param>
/// <param name="Email">E-mail to write to this person.</param>
/// <param name="Title">Title at the board (Président, Secrétaire, Trésorière…); null when the person holds none.</param>
public sealed record ListBoardMembersItem(string AccountId, string Name, string Email, string? Title);
