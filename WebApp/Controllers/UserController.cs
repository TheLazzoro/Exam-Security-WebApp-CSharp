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
                bool isJpeg;
                bool isPng;

                // Analyse file
                using (Stream s = file.OpenReadStream())
                {
                    BinaryReader reader = new BinaryReader(s);

                    // JFIF markers
                    UInt16 soi = reader.ReadUInt16();    // Start of Image (SOI) marker (FFD8)
                    UInt16 marker = reader.ReadUInt16(); // JFIF marker (FFE0) or EXIF marker(FFE1)

                    // PNG markers
                    reader.BaseStream.Position = 0; // reset read position.
                    UInt64 pngHeader = reader.ReadUInt64();

                    isJpeg = soi == 0xd8ff && (marker & 0xe0ff) == 0xe0ff;
                    isPng = pngHeader == 0x0a1a0a0d474e5089;

                    bool isFileValid = isJpeg || isPng;
                    if (!isFileValid)
                    {
                        throw new API_Exception(HttpStatusCode.BadRequest, "Invalid file type.");
                    }

                    reader.Dispose();
                }

                // Save file

                var user = Token.GetCurrentUser(HttpContext);

                string specialFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string username = user.Username;
                string dir = Path.Combine(specialFolder, username);

                // re-construct file extension
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                string ext = string.Empty;

                if (isJpeg)
                    ext = ".jpg";
                else if (isPng)
                    ext = ".png";

                fileName += ext;
                string fullpath = Path.Combine(dir, fileName);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (FileStream fileStream = System.IO.File.Create(fullpath))
                {
                    await file.CopyToAsync(fileStream);
                }


                return Ok();
            }
            catch (API_Exception ex)
            {
                throw new API_Exception(HttpStatusCode.BadRequest, "Invalid file type.");
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
