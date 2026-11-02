namespace BadPig.Profile.Features.SetMyLicence;

/// <summary>JSON body of the change of OWN licence number.</summary>
/// <param name="Number">Licence number (letters, digits and dashes, 20 characters at most); empty removes the one on record.</param>
public sealed record SetMyLicenceRequest(string? Number);
