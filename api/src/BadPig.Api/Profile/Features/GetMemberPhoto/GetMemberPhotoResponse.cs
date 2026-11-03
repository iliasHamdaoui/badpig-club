namespace BadPig.Profile.Features.GetMemberPhoto;

/// <summary>The profile picture of a person, ready to be sent.</summary>
/// <param name="Content">The bytes of the image.</param>
/// <param name="ContentType">Media type (image/jpeg, image/png or image/webp).</param>
public sealed record GetMemberPhotoResponse(byte[] Content, string ContentType);
