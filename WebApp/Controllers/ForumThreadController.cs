using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.DTOS;
using WebApp.Facades;
using WebApp.Model;
using WebApp.Utility;

namespace WebApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ForumThreadController : Controller
    {
        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] ForumThreadDTO dto)
        {
            var currentUser = Token.GetCurrentUser(HttpContext); // Get user from token rather than from the dto.

            ForumThread forumThread = new ForumThread(dto.Title, dto.Content, currentUser);
            ForumThreadFacade.Create(forumThread);

            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            ForumThread? forumThread = await ForumThreadFacade.Get(id);
            var dto = new ForumThreadDTO(forumThread);
            return Ok(dto);
        }

        [HttpGet]
        public async Task<IEnumerable<ForumThreadDTO>> GetAll()
        {
            var forumThreads = await ForumThreadFacade.GetAll();
            return forumThreads.ToArray();
        }
    }
}
