using System.Text.Json;

namespace LiftTracker.Infrastructure.Authentication;

/// <summary>
/// Represents user profile information from Google OAuth
/// </summary>
public class GoogleUserProfile
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public bool VerifiedEmail { get; set; }
}

/// <summary>
/// Service for handling Google OAuth authentication
/// </summary>
public class GoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleAuthOptions _authOptions;

    public GoogleAuthService(HttpClient httpClient, Microsoft.Extensions.Options.IOptions<GoogleAuthOptions> authOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authOptions = authOptions.Value ?? throw new ArgumentNullException(nameof(authOptions));
    }

    /// <summary>
    /// Verifies a Google OAuth access token and returns user profile
    /// </summary>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <returns>Google user profile if valid, null otherwise</returns>
    public async Task<GoogleUserProfile?> VerifyGoogleTokenAsync(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            return null;

        try
        {
            // Call Google's userinfo endpoint to verify the token and get user profile
            var response = await _httpClient.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var userProfile = JsonSerializer.Deserialize<GoogleUserProfile>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            return userProfile;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Verifies a Google ID token and returns user profile
    /// </summary>
    /// <param name="idToken">Google ID token</param>
    /// <returns>Google user profile if valid, null otherwise</returns>
    public async Task<GoogleUserProfile?> VerifyGoogleIdTokenAsync(string idToken)
    {
        if (string.IsNullOrEmpty(idToken))
            return null;

        try
        {
            // Call Google's tokeninfo endpoint to verify the ID token
            var response = await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var tokenInfo = JsonSerializer.Deserialize<JsonElement>(json);

            // Verify the audience (client ID)
            if (!tokenInfo.TryGetProperty("aud", out var audElement) ||
                audElement.GetString() != _authOptions.ClientId)
                return null;

            // Extract user profile information
            var userProfile = new GoogleUserProfile();

            if (tokenInfo.TryGetProperty("sub", out var subElement))
                userProfile.Id = subElement.GetString() ?? string.Empty;

            if (tokenInfo.TryGetProperty("email", out var emailElement))
                userProfile.Email = emailElement.GetString() ?? string.Empty;

            if (tokenInfo.TryGetProperty("name", out var nameElement))
                userProfile.Name = nameElement.GetString() ?? string.Empty;

            if (tokenInfo.TryGetProperty("given_name", out var givenNameElement))
                userProfile.GivenName = givenNameElement.GetString() ?? string.Empty;

            if (tokenInfo.TryGetProperty("family_name", out var familyNameElement))
                userProfile.FamilyName = familyNameElement.GetString() ?? string.Empty;

            if (tokenInfo.TryGetProperty("picture", out var pictureElement))
                userProfile.Picture = pictureElement.GetString() ?? string.Empty;

            if (tokenInfo.TryGetProperty("email_verified", out var verifiedElement))
                userProfile.VerifiedEmail = verifiedElement.GetBoolean();

            return userProfile;
        }
        catch
        {
            return null;
        }
    }
}
