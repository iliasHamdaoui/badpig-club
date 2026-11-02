using System.Collections.Concurrent;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;

namespace BadPig.Api.Tests.Infrastructure;

/// <summary>
/// The accounts known to the substituted identity provider. Tests add accounts here (the situation), and the NSubstitute
/// substitute of <see cref="IIdentityProvider"/> answers from it and applies the changes it is asked to make,
/// so that what the API did can be read back (<see cref="Get"/>) and the calls can be checked with <c>Received()</c>.
/// </summary>
public sealed class AccountDirectory
{
    private readonly ConcurrentDictionary<string, AccountInfo> _accounts = new();

    public void Add(AccountInfo account) => _accounts[account.Id] = account;

    public AccountInfo? Get(string id) => _accounts.GetValueOrDefault(id);

    public AccountInfo? FindByEmail(string email) =>
        _accounts.Values.FirstOrDefault(a => string.Equals(a.Email, email, StringComparison.OrdinalIgnoreCase));

    public void Clear() => _accounts.Clear();

    /// <summary>Makes the substitute behave like an identity provider that keeps its accounts in this directory.</summary>
    public void Wire(IIdentityProvider identity)
    {
        identity.CanSendSignupLink.Returns(true);

        identity.FindAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(Get(call.Arg<string>())));

        identity.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(FindByEmail(call.Arg<string>())));

        identity.ListAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<IReadOnlyList<AccountInfo>>(_accounts.Values.OrderBy(a => a.Name).ToList()));

        identity.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyCollection<ClubGroup>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var email = call.ArgAt<string>(1);
                var groups = call.ArgAt<IReadOnlyCollection<ClubGroup>>(2).Select(Groups.NameOf).ToArray();
                var account = new AccountInfo(Guid.NewGuid().ToString(), email, call.ArgAt<string>(0), email, true, groups, null);
                Add(account);
                return Task.FromResult(account);
            });

        identity.UpdateProfileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var updated = _accounts[call.ArgAt<string>(0)] with { Name = call.ArgAt<string>(1), Email = call.ArgAt<string>(2) };
                Add(updated);
                return Task.FromResult(updated);
            });

        identity.SetActiveAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                Add(_accounts[call.Arg<string>()] with { IsActive = call.Arg<bool>() });
                return Task.CompletedTask;
            });

        identity.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                _accounts.TryRemove(call.Arg<string>(), out _);
                return Task.CompletedTask;
            });

        identity.AddToGroupAsync(Arg.Any<string>(), Arg.Any<ClubGroup>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var account = _accounts[call.Arg<string>()];
                Add(account with { Groups = account.Groups.Append(Groups.NameOf(call.Arg<ClubGroup>())).Distinct().ToArray() });
                return Task.CompletedTask;
            });

        identity.RemoveFromGroupAsync(Arg.Any<string>(), Arg.Any<ClubGroup>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var account = _accounts[call.Arg<string>()];
                Add(account with { Groups = account.Groups.Where(g => g != Groups.NameOf(call.Arg<ClubGroup>())).ToArray() });
                return Task.CompletedTask;
            });
    }
}
