namespace BadPig.Profile.Features.SetMyPhoto;

/// <summary>Sets the profile picture of the signed-in person (the account comes from the token).</summary>
/// <param name="File">The picture sent (JPEG, PNG or WebP, 1 MB at most).</param>
public sealed record SetMyPhotoRequest(UploadedPhoto? File);
