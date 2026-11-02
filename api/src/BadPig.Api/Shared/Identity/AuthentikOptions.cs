using BadPig.Shared.Core;

namespace BadPig.Shared.Identity;

/// <summary>Authentik settings. The API token never comes from a file: environment variable only.</summary>
internal sealed class AuthentikOptions
{
    public const string Section = "Authentik";

    /// <summary>Token issuer, with the trailing "/".</summary>
    public string Issuer { get; set; } = "";

    /// <summary>Expected audience: the Client ID of the site's OIDC provider.</summary>
    public string Audience { get; set; } = "";

    /// <summary>Internal address of the Authentik API (Docker network "proxy").</summary>
    public string ApiBase { get; set; } = "";

    /// <summary>Identifier (UUID) of the "Membres" group: invited people are placed in it.</summary>
    public string MembersGroup { get; set; } = "";

    /// <summary>Identifier (UUID) of the "Bureau" group.</summary>
    public string BoardGroup { get; set; } = "";

    /// <summary>Identifier (UUID) of the "Admin" group (only needed to invite an administrator).</summary>
    public string AdminGroup { get; set; } = "";

    /// <summary>Identifier (UUID) of the invitation e-mail stage (the "bienvenue" template).</summary>
    public string InvitationStage { get; set; } = "";

    /// <summary>Configured Authentik identifier (UUID text, may be empty) of a club group.</summary>
    public string GroupId(ClubGroup group) => group switch
    {
        ClubGroup.Members => MembersGroup,
        ClubGroup.Board => BoardGroup,
        ClubGroup.Admin => AdminGroup,
        _ => "",
    };

    /// <summary>Service account token (type "API Token").</summary>
    public string ApiToken { get; set; } = "";
}
