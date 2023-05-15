using WebApp.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using WebApp.ErrorHandling;
using WebApp.Model;
using WebApp.Facades;
using WebApp.Utility;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "user")]
        public IEnumerable<string> Get()
        {
            var currentUser = Token.GetCurrentUser(HttpContext);

            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public UserSafeDTO Get(long id)
        {
            var user = UserFacade.Get(id);
            return new UserSafeDTO(user);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserDTO dto)
        {
            //UserDTO dto = JsonConvert.DeserializeObject<UserDTO>(value);
            User user = new User(dto.Username, dto.Password, "user");
            UserFacade.Create(user);

            var msg = new MessageDTO()
            {
                Message = "Account successfully created!",
                StatusCode = 200
            };

            return Ok(msg);
        }

    }
}
