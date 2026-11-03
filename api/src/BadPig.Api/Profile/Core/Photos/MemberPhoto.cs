namespace BadPig.Profile.Core;

/// <summary>The profile picture of a person (one at most), kept in its own table so that the lists never read the bytes.</summary>
public sealed class MemberPhoto
{
    public Guid AccountId { get; set; }
    public string ContentType { get; set; } = "";
    public byte[] Content { get; set; } = [];

    /// <summary>Last change; its Unix time in milliseconds is the version that makes the address of the picture change when it is replaced.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
