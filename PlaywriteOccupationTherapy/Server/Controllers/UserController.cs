using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PlaywriteOccupationTherapy.Client.Shared.Models;
using PlaywriteOccupationTherapy.Shared.Models;
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
        [RequireHttps]
        public async Task<ActionResult<LoginModel>> LoginUser([FromBody] LoginModel user)
        {
            using SqlConnection conn = new(_configuration.GetConnectionString("PlaywriteDB"));
            using SqlCommand command = new("PWD.PROC_LOGIN", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
            _ = user.Username.Trim();
            _ = user.Password.Trim();
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
        public async Task<ActionResult<Users>> GetCurrentUser()
        {
            Users user = new();
            if (User.Identity.IsAuthenticated)
            {
                user.Email = User.FindFirstValue(ClaimTypes.Name);
                using SqlConnection conn = new(_configuration.GetConnectionString("PlaywriteDB"));
                using SqlCommand command = new("SELECT Id, FirstName, LastName, IsAdmin, IsSuperAdmin, IsGeneralUser FROM PWD.USERS WHERE Email = '"+user.Email+"';", conn);
                conn.Open();
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    user.Id = (int)reader["Id"];
                    user.FirstName = reader["FirstName"].ToString();
                    user.LastName = reader["LastName"].ToString();
                    user.IsAdmin = (bool?)reader["IsAdmin"];
                    user.IsSuperAdmin = (bool?)reader["IsSuperAdmin"];
                    user.IsGeneralUser = (bool?)reader["IsGeneralUser"];
                }
            }
            return await Task.FromResult(user);
        }

        [HttpPost]
        public async Task<ActionResult<RegisterModel>> RegisterUser(RegisterModel registerModel)
        {
            try
            {
                using SqlConnection conn = new(_configuration.GetConnectionString("PlaywriteDB"));
                conn.Open();

                using SqlCommand command = new("PWD.PROC_REGISTER", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@firstName", registerModel.FirstName);
                command.Parameters.AddWithValue("@lastName", registerModel.LastName);
                command.Parameters.AddWithValue("@email", registerModel.Email);
                command.Parameters.AddWithValue("@password", registerModel.Password);
                command.Parameters.AddWithValue("@generaluser", 1);
                command.Parameters.AddWithValue("@admin", 0);
                command.Parameters.AddWithValue("@superadmin", 0);
                await command.ExecuteNonQueryAsync();

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async void LogoutUser()
        {
            await HttpContext.SignOutAsync();
        }
    }
}
