using Microsoft.AspNetCore.Components.Authorization;
using PlaywriteOccupationTherapy.Client.Shared.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PlaywriteOccupationTherapy.Client.Shared.Authentication
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;

        public CustomAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            LoginModel currentUser = await _httpClient.GetFromJsonAsync<LoginModel>("api/user/getcurrentuser");

            if (currentUser != null && currentUser.Username != null)
            {
                Claim claim = new(ClaimTypes.Name, currentUser.Username);
                ClaimsIdentity claimsIdentity = new(new[] { claim }, "serverAuth");
                ClaimsPrincipal claimsPrincipal = new(claimsIdentity);

                return new AuthenticationState(claimsPrincipal);
            }
            else
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
    }
}
