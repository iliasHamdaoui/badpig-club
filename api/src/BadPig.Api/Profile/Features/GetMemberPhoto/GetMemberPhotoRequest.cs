namespace BadPig.Profile.Features.GetMemberPhoto;

/// <summary>The profile picture of a person.</summary>
/// <param name="Id">Account identifier of the person.</param>
public sealed record GetMemberPhotoRequest(string Id);
