using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MultiTenantApp.Web.Interfaces;

namespace MultiTenantApp.Web.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ITokenProvider _tokenProvider;

        public CustomAuthenticationStateProvider(ILocalStorageService localStorage, ITokenProvider tokenProvider)
        {
            _localStorage = localStorage;
            _tokenProvider = tokenProvider;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var savedToken = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(savedToken))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Set the token in the TokenProvider so AuthenticatedHttpClient can use it
                await _tokenProvider.SetTokenAsync(savedToken);

                var claims = ParseClaimsFromJwt(savedToken).ToList();
                
                // Add permissions as roles
                var permissions = await _localStorage.GetItemAsync<IEnumerable<string>>("userPermissions");
                if (permissions != null)
                {
                    foreach (var permission in permissions)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, permission));
                        
                        // If user has Admin rule, ensure they have Admin role
                        if (permission == "Admin")
                        {
                            // It's already added as a role above, but just to be explicit if we need special handling
                        }
                    }
                }

                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
            }
            catch (InvalidOperationException)
            {
                // JavaScript interop not available during prerendering
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Exception)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void MarkUserAsAuthenticated(string token)
        {
            // We can't easily get permissions here synchronously without making this async, 
            // but NotifyAuthenticationStateChanged expects a Task<AuthenticationState>.
            // Ideally, we should reload the state fully.
            // However, for immediate UI update, we might miss permissions until a refresh if we don't handle it.
            // Let's try to just trigger a re-fetch by calling GetAuthenticationStateAsync indirectly via a new state.
            
            // Actually, since we just logged in, we know we stored permissions. 
            // But we can't access LocalStorage synchronously here if we wanted to build the principal manually.
            // The best way is to let the app re-authorize.
            
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString() ?? ""));
                }
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return System.Convert.FromBase64String(base64);
        }
    }
}
