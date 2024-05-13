namespace SearchAiDirectory.Services;

public class JWTokenService(string baseURL)
{
    private readonly static string key = "ThisIsTheSearchAiDirectioryAppPrivateKeyWhichIsVeryLongVeryVeryLongKey.";

    public string AuthenticateUser(User user)
    {
        if (user is null) return string.Empty;

        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new(ClaimTypes.Role, user.Role.Name),
                new(ClaimTypes.Name, $"{user.Name}"),
                new(ClaimTypes.Email, user.Email)
            };

        if (!string.IsNullOrEmpty(user.TimeZone))
            claims.Add(new Claim(ClaimTypes.Locality, user.TimeZone));

        //Create Token With Descriptor
        JwtSecurityTokenHandler tokenHandler = new();
        byte[] tokenKey = Encoding.ASCII.GetBytes(key);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature),
            IssuedAt = DateTime.UtcNow.AddMinutes(-1),
            NotBefore = DateTime.UtcNow.AddMinutes(-2),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = baseURL
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

internal static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        List<Claim> claims = [
            .. keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()))
            ];

        return claims;
    }
    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
            default:
                break;
        }
        return Convert.FromBase64String(base64);
    }
}