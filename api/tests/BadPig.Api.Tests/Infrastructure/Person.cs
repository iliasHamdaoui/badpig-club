namespace BadPig.Api.Tests.Infrastructure;

/// <summary>A person of the identity provider who can sign in to the tests (see <see cref="IntegrationTest.As"/>).</summary>
public sealed record Person(string Id, string Name, string Email, string[] Groups);
