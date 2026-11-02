namespace BadPig.Shared.Core;

/// <summary>
/// Implemented by every module that stores personal data about an account: when the account is deleted
/// (by its owner, by the board or automatically), each module erases its own data (right to erasure).
/// </summary>
public interface IAccountErasure
{
    Task EraseAsync(string accountId, CancellationToken ct);
}
