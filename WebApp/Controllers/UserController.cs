using WebApp.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using WebApp.ErrorHandling;
using WebApp.Model;
using WebApp.Facades;
using WebApp.Utility;
using System.Net.Mime;

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
        [RequestSizeLimit(1024 * 1024 * 10)] // 10 MB
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if(!file.ContentType.StartsWith("image/"))
            {
                return BadRequest("Only images are supported.");
            }

            byte[] buffer = new byte[file.Length];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                Stream s = file.OpenReadStream();
                while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
            }
            await UserFacade.UploadImage(buffer, HttpContext);
            var msg = new ResponseDTO()
            {
                Message = "Uploaded image.",
                StatusCode = 200
            };

            return Ok(msg);
        }

        [HttpGet("Image-Get/{userId}")]
        public async Task<IActionResult> GetUserImage(long userId)
        {
            string path = UserFacade.GetUserImagePath(userId, HttpContext);
            var msg = new ResponseDTO()
            {
                StatusCode = 200,
                Message = path
            };

            return Ok(msg);
        }

        [HttpGet("Image-Get-By-Username/{username}")]
        public async Task<IActionResult> GetUserImage(string username)
        {
            var user = UserFacade.Get(username);
            if(user == null)
            {
                throw new API_Exception(HttpStatusCode.NotFound, $"User '{username}' was not found.");
            }
            string path = UserFacade.GetUserImagePath(user.Id, HttpContext);
            var msg = new ResponseDTO()
            {
                StatusCode = 200,
                Message = path
            };

            return Ok(msg);
        }
    }
}
