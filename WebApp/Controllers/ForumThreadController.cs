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
        private ILogger _logger;
        private static RequestLimiter requestLimiter = new RequestLimiter(10, 60);

        public ForumThreadController(ILogger<ForumThreadController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> Create([FromBody] ForumThreadDTO dto)
        {
            var currentUser = Token.GetCurrentUser(HttpContext); // Get user from token rather than from the dto.
            bool canRequest = await requestLimiter.OnRequest(currentUser.Username, HttpContext, _logger);
            if (!canRequest)
            {
                return BadRequest();
            }

            ForumThread forumThread = new ForumThread(dto.Title, dto.Content, currentUser);
            var facade = new ForumThreadFacade(_logger);
            facade.Create(forumThread);

            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var facade = new ForumThreadFacade(_logger);
            ForumThread? forumThread = await facade.Get(id);
            var dto = new ForumThreadDTO(forumThread);
            return Ok(dto);
        }

        [HttpGet]
        public async Task<IEnumerable<ForumThreadDTO>> GetAll()
        {
            var facade = new ForumThreadFacade(_logger);
            var forumThreads = await facade.GetAll();
            return forumThreads.ToArray();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var user = Token.GetCurrentUser(HttpContext);
            var facade = new ForumThreadFacade(_logger);
            await facade.Delete(user, id);

            return Ok();
        }
    }
}
