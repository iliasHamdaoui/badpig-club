
namespace BadPig.Profile.Features.UpdateMyProfile;

/// <summary>Body of the profile update.</summary>
/// <param name="Name">Full name (1 to 200 characters).</param>
/// <param name="Email">E-mail address (at most 254 characters).</param>
public sealed record UpdateMyProfileRequest(
    string? Name,
    string? Email);
