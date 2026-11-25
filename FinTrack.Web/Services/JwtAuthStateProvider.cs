using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace FinTrack.Web.Services;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStore _tokens;
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthStateProvider(TokenStore tokens) => _tokens = tokens;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokens.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        if (IsExpired(token))
        {
            await _tokens.ClearAsync();
            return Anonymous;
        }

        var identity = new ClaimsIdentity(ParseClaims(token), "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyUserChanged()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static bool IsExpired(string token)
    {
        var exp = GetClaimValue(token, "exp");
        if (long.TryParse(exp, out var seconds))
            return DateTimeOffset.FromUnixTimeSeconds(seconds) <= DateTimeOffset.UtcNow;
        return false;
    }

    private static string? GetClaimValue(string token, string key)
    {
        foreach (var claim in ParseClaims(token))
            if (claim.Type == key) return claim.Value;
        return null;
    }

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2) yield break;

        var json = DecodeBase64Url(parts[1]);
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        if (dict is null) yield break;

        foreach (var kv in dict)
        {
            var value = kv.Value.ValueKind == JsonValueKind.String
                ? kv.Value.GetString() ?? string.Empty
                : kv.Value.ToString();
            yield return new Claim(kv.Key, value);

            if (kv.Key == "name")
                yield return new Claim(ClaimTypes.Name, value);
            if (kv.Key == "email")
                yield return new Claim(ClaimTypes.Email, value);
        }
    }

    private static byte[] DecodeBase64Url(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}
