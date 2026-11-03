namespace BadPig.Profile.Features.ListClubDirectory;

/// <summary>One person of the club directory.</summary>
/// <param name="Id">Account identifier of the person (the picture is served at <c>/members/{id}/photo?v={photoVersion}</c>).</param>
/// <param name="Name">Full name.</param>
/// <param name="PhotoVersion">Version of the profile picture (changes when the picture is replaced): the address of the picture carries it so the browser may keep the picture.</param>
public sealed record ListClubDirectoryItem(string Id, string Name, long PhotoVersion);
