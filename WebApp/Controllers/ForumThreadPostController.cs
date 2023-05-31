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
    public class ForumThreadPostController : Controller
    {
        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] ForumThreadPostDTO dto)
        {
            var currentUser = Token.GetCurrentUser(HttpContext); // Get user from token rather than from the dto.

            ForumThreadPost forumThreadPost = new ForumThreadPost(dto.Content, currentUser, dto.ThreadId);
            ForumThreadPostFacade.Create(forumThreadPost);

            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var forumThreadPost = await ForumThreadPostFacade.Get(id);
            var dto = new ForumThreadPostDTO(forumThreadPost);
            return Ok(dto);
        }

        [HttpGet("Thread/{id}")]
        public async Task<IEnumerable<ForumThreadPostDTO>> GetByThreadId(long id)
        {
            var threadPosts = await ForumThreadPostFacade.GetByThreadId(id); 
            return threadPosts.ToArray();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var user = Token.GetCurrentUser(HttpContext);
            await ForumThreadPostFacade.Delete(user, id);

            return Ok();
        }
    }
}
