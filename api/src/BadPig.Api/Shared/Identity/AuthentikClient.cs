using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace BadPig.Shared.Identity;

/// <summary>Calls to the Authentik API with the service account (permissions: see README).</summary>
internal sealed class AuthentikClient(HttpClient http, IOptions<AuthentikOptions> options)
{
    public AuthentikOptions Options => options.Value;

    /// <summary>Finds a user by UUID (the "sub" value of the token).</summary>
    public async Task<AuthentikUser?> FindByUuid(string uuid, CancellationToken ct)
    {
        if (!Guid.TryParse(uuid, out var guid))
            return null;

        var list = await http.GetFromJsonAsync<AuthentikList>($"core/users/?uuid={guid}&include_groups=true", ct);
        return list?.Results.FirstOrDefault();
    }

    public async Task<List<AuthentikUser>> ListUsers(CancellationToken ct)
    {
        // All account types (members are external, board members may be internal);
        // technical accounts are excluded later by the group filter.
        var list = await http.GetFromJsonAsync<AuthentikList>(
            "core/users/?include_groups=true&ordering=name&page_size=500", ct);
        return list?.Results ?? [];
    }

    /// <summary>Finds an account by e-mail address (exact match, case-insensitive).</summary>
    public async Task<AuthentikUser?> FindByEmail(string email, CancellationToken ct)
    {
        var list = await http.GetFromJsonAsync<AuthentikList>($"core/users/?email={Uri.EscapeDataString(email)}&page_size=5", ct);
        return list?.Results.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Creates an active external account (no access to the Authentik UI) without a password, in the given groups. The e-mail address is the username.</summary>
    public Task<AuthentikUser> CreateUser(string name, string email, Guid[] groups, CancellationToken ct) =>
        Send(HttpMethod.Post, "core/users/",
            new { username = email, name, email, is_active = true, type = "external", path = "users", groups }, ct);

    /// <summary>Sends the person an e-mail with a link (valid 3 days) to choose a password.</summary>
    public async Task SendSignupLink(int pk, Guid emailStage, CancellationToken ct)
    {
        using var response = await Request(HttpMethod.Post, $"core/users/{pk}/recovery_email/",
            new { email_stage = emailStage, token_duration = "days=3" }, ct);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>Adds an account to a group (requires the "Add user to group" permission).</summary>
    public async Task AddToGroup(Guid group, int pk, CancellationToken ct)
    {
        using var response = await Request(HttpMethod.Post, $"core/groups/{group}/add_user/", new { pk }, ct);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>Removes an account from a group (requires the "Remove user from group" permission).</summary>
    public async Task RemoveFromGroup(Guid group, int pk, CancellationToken ct)
    {
        using var response = await Request(HttpMethod.Post, $"core/groups/{group}/remove_user/", new { pk }, ct);
        response.EnsureSuccessStatusCode();
    }

    public Task<AuthentikUser> UpdateNameAndEmail(int pk, string name, string email, CancellationToken ct) =>
        Patch(pk, new { name, email }, ct);

    /// <summary>Disables or re-enables an account (a disabled account cannot sign in).</summary>
    public Task<AuthentikUser> SetActive(int pk, bool active, CancellationToken ct) =>
        Patch(pk, new { is_active = active }, ct);

    /// <summary>Permanently deletes an account (requires the "Delete user" permission).</summary>
    public async Task Delete(int pk, CancellationToken ct)
    {
        using var response = await http.DeleteAsync($"core/users/{pk}/", ct);
        response.EnsureSuccessStatusCode();
    }

    private Task<AuthentikUser> Patch(int pk, object body, CancellationToken ct) =>
        Send(HttpMethod.Patch, $"core/users/{pk}/", body, ct);

    private async Task<AuthentikUser> Send(HttpMethod method, string address, object body, CancellationToken ct)
    {
        using var response = await Request(method, address, body, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthentikUser>(ct))!;
    }

    private async Task<HttpResponseMessage> Request(HttpMethod method, string address, object body, CancellationToken ct)
    {
        // The body is sent with a declared length (Content-Length): in "chunked" mode, Authentik ignores
        // the body and answers 200 without changing anything.
        var json = System.Text.Json.JsonSerializer.Serialize(body);
        using var request = new HttpRequestMessage(method, address)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        };
        return await http.SendAsync(request, ct);
    }
}
