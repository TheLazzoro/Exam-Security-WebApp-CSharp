using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using WebApp.DTOS;
using MySqlConnector;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using WebApp.Model;
using WebApp.Facades;
using System.Text.Json.Nodes;

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
            JsonObject jsonToken = new JsonObject();
            jsonToken.Add("token", token);
            jsonToken.Add("username", user.Username);

            return Ok(jsonToken);
        }

        private string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(SharedSecret.GetSharedKey());
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("id", user.Id.ToString()),
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
