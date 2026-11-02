namespace BadPig.Administration.Features.SetMemberLicence;

/// <summary>Change of the licence number of a person by the board.</summary>
/// <param name="Id">Account identifier of the person.</param>
/// <param name="Body">The new licence number.</param>
public sealed record SetMemberLicenceRequest(string Id, SetMemberLicenceBody Body);
