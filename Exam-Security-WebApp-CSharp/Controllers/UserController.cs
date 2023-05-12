using Exam_Security_WebApp_CSharp.DTOS;
using Facades;
using Microsoft.AspNetCore.Mvc;
using Model;
using Newtonsoft.Json;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Exam_Security_WebApp_CSharp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UserController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            UserDTO dto = JsonConvert.DeserializeObject<UserDTO>(value);
            User user = new User(dto.Username, dto.Password);
            UserFacade.Create(user);
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
