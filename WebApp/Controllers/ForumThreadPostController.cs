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
        private ILogger _logger;
        private static RequestLimiter requestLimiter = new RequestLimiter(10, 60);

        public ForumThreadPostController(ILogger<ForumThreadPostController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ForumThreadPostDTO dto)
        {
            var currentUser = Token.GetCurrentUser(HttpContext); // Get user from token rather than from the dto.
            bool canRequest = await requestLimiter.OnRequest(currentUser.Username, HttpContext, _logger);
            if (!canRequest)
            {
                return BadRequest();
            }

            var facade = new ForumThreadPostFacade(_logger);
            ForumThreadPost forumThreadPost = new ForumThreadPost(dto.Content, currentUser, dto.ThreadId);
            await facade.Create(forumThreadPost);

            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var facade = new ForumThreadPostFacade(_logger);
            var forumThreadPost = await facade.Get(id);
            var dto = new ForumThreadPostDTO(forumThreadPost);
            return Ok(dto);
        }

        [HttpGet("Thread/{id}")]
        public async Task<IEnumerable<ForumThreadPostDTO>> GetByThreadId(long id)
        {
            var facade = new ForumThreadPostFacade(_logger);
            var threadPosts = await facade.GetByThreadId(id); 
            return threadPosts.ToArray();
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Edit([FromBody] ForumThreadPostDTO dto)
        {
            var user = Token.GetCurrentUser(HttpContext);
            var facade = new ForumThreadPostFacade(_logger);
            await facade.Edit(user, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var user = Token.GetCurrentUser(HttpContext);
            var facade = new ForumThreadPostFacade(_logger);
            await facade.Delete(user, id);

            return Ok();
        }
    }
}
