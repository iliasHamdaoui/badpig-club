using BadPig.Shared.Core;

namespace BadPig.Shared.Infrastructure;

/// <summary><see cref="IUserContext"/> backed by the user of the current HTTP request.</summary>
internal sealed class HttpUserContext(IHttpContextAccessor accessor) : IUserContext
{
    private System.Security.Claims.ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public string? AccountId => Principal?.AccountId();

    public string DisplayName => Principal?.DisplayName() ?? "?";

    public bool IsInGroup(ClubGroup group) => Principal?.HasClaim("groups", Groups.NameOf(group)) ?? false;
}
