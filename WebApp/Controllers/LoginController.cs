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
using WebApp.ErrorHandling;
using WebApp.Utility;

namespace WebApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO userLogin)
        {
            Thread.Sleep(2000);

            var user = await Authenticate(userLogin);
            if (user == null)
            {
                return NotFound("Invalid login");
            }

            var token = Token.GenerateToken(user);
            JsonObject jsonToken = new JsonObject();
            jsonToken.Add("token", token);
            jsonToken.Add("username", user.Username);

            return Ok(jsonToken);
        }

        

        private async Task<User> Authenticate(UserDTO userLogin)
        {
            var username = userLogin.Username;
            var password = userLogin.Password;
            var user = UserFacade.Get(username);

            if (user == null)
            {
                throw new API_Exception(HttpStatusCode.NotFound, "Login failed.");
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
