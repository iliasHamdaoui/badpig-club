namespace BadPig.Shared.Infrastructure.Mail;

public sealed class SmtpOptions
{
    public const string Section = "Smtp";

    /// <summary>Sending server (the VPS's Postfix, reachable through the Docker network "mail"). Empty: no e-mail is sent.</summary>
    public string Host { get; set; } = "";
    public int Port { get; set; } = 25;
    public string From { get; set; } = "ne-pas-repondre@badpig.fr";
    public string FromName { get; set; } = "BadPig";
}
