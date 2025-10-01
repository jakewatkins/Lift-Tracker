using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.JSInterop;
using System.Text.Json;

namespace LiftTracker.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    private ClaimsPrincipal? _currentUser;
    private bool _isAuthenticated;

    public event Action<ClaimsPrincipal?>? AuthenticationStateChanged;

    public ClaimsPrincipal? CurrentUser => _currentUser;
    public bool IsAuthenticated => _isAuthenticated;

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        var token = await GetTokenFromStorageAsync();
        if (!string.IsNullOrEmpty(token) && IsTokenValid(token))
        {
            await SetAuthenticationAsync(token);
        }
        else
        {
            await ClearAuthenticationAsync();
        }
    }

    public async Task<bool> LoginWithGoogleAsync()
    {
        try
        {
            // Redirect to Google OAuth endpoint on the server
            var loginUrl = $"{_httpClient.BaseAddress}api/auth/google-login";
            await _jsRuntime.InvokeVoidAsync("window.location.href", loginUrl);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> HandleCallbackAsync(string code, string state)
    {
        try
        {
            var callbackRequest = new
            {
                Code = code,
                State = state
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/google-callback", callbackRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GoogleCallbackResponse>();
                if (result?.Token != null)
                {
                    await SetAuthenticationAsync(result.Token);
                    return true;
                }
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Call server logout endpoint
            await _httpClient.PostAsync("api/auth/logout", null);
        }
        catch (Exception)
        {
            // Continue with local logout even if server call fails
        }
        finally
        {
            await ClearAuthenticationAsync();
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        return await GetTokenFromStorageAsync();
    }

    public async Task RefreshTokenIfNeededAsync()
    {
        var token = await GetTokenFromStorageAsync();
        if (!string.IsNullOrEmpty(token) && IsTokenNearExpiry(token))
        {
            try
            {
                var response = await _httpClient.PostAsync("api/auth/refresh", null);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
                    if (result?.Token != null)
                    {
                        await SetAuthenticationAsync(result.Token);
                    }
                }
                else
                {
                    await ClearAuthenticationAsync();
                }
            }
            catch (Exception)
            {
                await ClearAuthenticationAsync();
            }
        }
    }

    private async Task SetAuthenticationAsync(string token)
    {
        _currentUser = CreateClaimsPrincipalFromToken(token);
        _isAuthenticated = true;

        await StoreTokenAsync(token);

        // Set authorization header for future requests
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        AuthenticationStateChanged?.Invoke(_currentUser);
    }

    private async Task ClearAuthenticationAsync()
    {
        _currentUser = null;
        _isAuthenticated = false;

        await RemoveTokenFromStorageAsync();

        // Clear authorization header
        _httpClient.DefaultRequestHeaders.Authorization = null;

        AuthenticationStateChanged?.Invoke(null);
    }

    private ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(jsonToken.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch (Exception)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }

    private bool IsTokenValid(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            return jsonToken.ValidTo > DateTime.UtcNow;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool IsTokenNearExpiry(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            return jsonToken.ValidTo <= DateTime.UtcNow.AddMinutes(5);
        }
        catch (Exception)
        {
            return true; // Assume expiry if we can't parse
        }
    }

    private async Task<string?> GetTokenFromStorageAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "auth_token");
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task StoreTokenAsync(string token)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "auth_token", token);
        }
        catch (Exception)
        {
            // Storage failed, but we can continue
        }
    }

    private async Task RemoveTokenFromStorageAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "auth_token");
        }
        catch (Exception)
        {
            // Storage removal failed, but we can continue
        }
    }
}

public class GoogleCallbackResponse
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime Expires { get; set; }
}

public class RefreshTokenResponse
{
    public string? Token { get; set; }
    public DateTime Expires { get; set; }
}
