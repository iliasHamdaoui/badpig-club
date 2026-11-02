using BadPig.Shared.Core;

namespace BadPig.Shared.Identity;

/// <summary>Authentik implementation of <see cref="IIdentityProvider"/> (internal: modules only see the interface).</summary>
internal sealed class AuthentikIdentityProvider(AuthentikClient authentik) : IIdentityProvider
{
    public bool CanSendSignupLink => Guid.TryParse(authentik.Options.InvitationStage, out _);

    public async Task<AccountInfo?> FindAsync(string accountId, CancellationToken ct) =>
        (await authentik.FindByUuid(accountId, ct))?.ToAccountInfo();

    public async Task<AccountInfo?> FindByEmailAsync(string email, CancellationToken ct) =>
        (await authentik.FindByEmail(email, ct))?.ToAccountInfo();

    public async Task<IReadOnlyList<AccountInfo>> ListAsync(CancellationToken ct) =>
        (await authentik.ListUsers(ct)).Select(u => u.ToAccountInfo()).ToList();

    public async Task<AccountInfo> CreateAsync(string name, string email, IReadOnlyCollection<ClubGroup> groups, CancellationToken ct)
    {
        var ids = groups.Select(GroupId).ToArray();
        return (await authentik.CreateUser(name, email, ids, ct)).ToAccountInfo();
    }

    public async Task<AccountInfo> UpdateProfileAsync(string accountId, string name, string email, CancellationToken ct) =>
        (await authentik.UpdateNameAndEmail((await Load(accountId, ct)).Pk, name, email, ct)).ToAccountInfo();

    public async Task SetActiveAsync(string accountId, bool active, CancellationToken ct) =>
        await authentik.SetActive((await Load(accountId, ct)).Pk, active, ct);

    public async Task DeleteAsync(string accountId, CancellationToken ct) =>
        await authentik.Delete((await Load(accountId, ct)).Pk, ct);

    public async Task AddToGroupAsync(string accountId, ClubGroup group, CancellationToken ct) =>
        await authentik.AddToGroup(GroupId(group), (await Load(accountId, ct)).Pk, ct);

    public async Task RemoveFromGroupAsync(string accountId, ClubGroup group, CancellationToken ct) =>
        await authentik.RemoveFromGroup(GroupId(group), (await Load(accountId, ct)).Pk, ct);

    public async Task SendSignupLinkAsync(string accountId, CancellationToken ct)
    {
        if (!Guid.TryParse(authentik.Options.InvitationStage, out var stage))
            throw new IdentityConfigurationException("Missing configuration: Authentik__InvitationStage");

        await authentik.SendSignupLink((await Load(accountId, ct)).Pk, stage, ct);
    }

    private async Task<AuthentikUser> Load(string accountId, CancellationToken ct) =>
        await authentik.FindByUuid(accountId, ct) ?? throw new InvalidOperationException("Unknown account");

    private Guid GroupId(ClubGroup group) =>
        Guid.TryParse(authentik.Options.GroupId(group), out var id)
            ? id
            : throw new IdentityConfigurationException($"Authentik group not configured: {group}");
}
