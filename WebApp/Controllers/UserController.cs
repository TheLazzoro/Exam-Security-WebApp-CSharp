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
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public UserSafeDTO Get(long id)
        {
            var user = UserFacade.Get(id);
            return new UserSafeDTO(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDTO dto)
        {
            //UserDTO dto = JsonConvert.DeserializeObject<UserDTO>(value);
            User user = new User(dto.Username, dto.Password, "user");
            await UserFacade.Create(user);

            var msg = new ResponseDTO()
            {
                Message = "Account successfully created!",
                StatusCode = 200
            };

            return Ok(msg);
        }


        [HttpPost]
        [Route("Image-Upload")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            UserFacade.UploadImage(file, HttpContext);
            var msg = new ResponseDTO()
            {
                Message = "Uploaded image.",
                StatusCode = 200
            };

            return Ok(msg);
        }
    }
}
