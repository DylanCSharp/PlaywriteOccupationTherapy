using Microsoft.AspNetCore.Components.Authorization;
using PlaywriteOccupationTherapy.Client.Shared.Models;
using PlaywriteOccupationTherapy.Shared.Models;
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
            Users currentUser = await _httpClient.GetFromJsonAsync<Users>("api/user/getcurrentuser");

            if (currentUser != null && currentUser.FirstName != null)
            {
                Claim claim = new(ClaimTypes.Name, currentUser.FirstName);
                Claim claimTwo = new(ClaimTypes.Email, currentUser.Email);

                ClaimsIdentity claimsIdentity = new(new[] { claim, claimTwo }, "serverAuth");
                if (currentUser.IsAdmin == true)
                {
                    Claim claimThree = new(ClaimTypes.Role, "GeneralAdmin");
                    claimsIdentity.AddClaim(claimThree);
                }
                if (currentUser.IsSuperAdmin == true)
                {
                    Claim claimFour = new(ClaimTypes.Role, "SuperAdmin");
                    claimsIdentity.AddClaim(claimFour);
                }
                if (currentUser.IsGeneralUser == true)
                {
                    Claim claimFive = new(ClaimTypes.Role, "GeneralUser");
                    claimsIdentity.AddClaim(claimFive);
                }
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
