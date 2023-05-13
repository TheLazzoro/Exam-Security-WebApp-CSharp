using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using WebApp.DTOS;
using Model;
using MySqlConnector;
using Database;
using Facades;
using System.Net;
using Microsoft.IdentityModel.Tokens;

namespace WebApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private static readonly int TokenLifetimeHours = 2;

        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserDTO userLogin)
        {
            var user = Authenticate(userLogin);
            if (user == null)
            {
                return NotFound("Invalid login");
            }

            var token = GenerateToken(user);
            return Ok(token);
        }

        private string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(SharedSecret.GetSharedKey());
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var token = new JwtSecurityToken(
                null, //_config["Jwt:Issuer"],
                null, //_config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(TokenLifetimeHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private User Authenticate(UserDTO userLogin)
        {
            var username = userLogin.Username;
            var password = userLogin.Password;
            var user = UserFacade.Get(username);

            if (user == null)
            {
                throw new System.Web.Http.HttpResponseException(HttpStatusCode.NotFound);
            }

            if (user.VerifyPassword(password))
            {
                return user;
            }
            else
            {
                return null;
            }
        }
    }
}
