namespace BadPig.Administration.Features.SetMemberLicence;

/// <summary>JSON body of the change of a licence number.</summary>
/// <param name="Number">Licence number (letters, digits and dashes, 20 characters at most); empty removes the one on record.</param>
public sealed record SetMemberLicenceBody(string? Number);
