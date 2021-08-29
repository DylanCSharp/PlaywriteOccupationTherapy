using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PlaywriteOccupationTherapy.Client.Shared.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PlaywriteOccupationTherapy.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<LoginModel>> LoginUser(LoginModel user)
        {
            using SqlConnection conn = new(_configuration.GetConnectionString("PlaywriteDB"));
            using SqlCommand command = new("PWD.PROC_LOGIN", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
            command.Parameters.AddWithValue("@email", user.Username);
            command.Parameters.AddWithValue("@password", user.Password);
            int loginResult = Convert.ToInt32(command.ExecuteScalar());
            if (loginResult == 1)
            {
                Claim claim = new(ClaimTypes.Name, user.Username);
                ClaimsIdentity claimsIdentity = new(new[] { claim }, "serverAuth");
                ClaimsPrincipal claimsPrincipal = new(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                return user;
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<ActionResult<LoginModel>> GetCurrentUser()
        {
            LoginModel user = new();
            if (User.Identity.IsAuthenticated)
            {
                user.Username = User.FindFirstValue(ClaimTypes.Name);
            }
            return await Task.FromResult(user);
        }

        [HttpGet]
        public async void LogoutUser()
        {
            await HttpContext.SignOutAsync();
        }
    }
}
