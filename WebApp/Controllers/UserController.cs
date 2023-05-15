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
        public IActionResult CreateUser([FromBody] UserDTO dto)
        {
            //UserDTO dto = JsonConvert.DeserializeObject<UserDTO>(value);
            User user = new User(dto.Username, dto.Password, "user");
            UserFacade.Create(user);

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
            try
            {
                using (Stream s = file.OpenReadStream())
                {
                    // Analyse file
                    BinaryReader reader = new BinaryReader(s);
                    var jpeg_marker = reader.ReadBytes(3);

                    await file.OpenReadStream().CopyToAsync(s);

                    // Save file

                    var user = Token.GetCurrentUser(HttpContext);
                    string specialFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string username = user.Username;

                    string path = "C:/Users/Lasse Dam/Desktop/MyFile";
                    string dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }


                return Ok();
            }
            catch (Exception ex)
            {
                return Problem();
            }

        }
    }
}
