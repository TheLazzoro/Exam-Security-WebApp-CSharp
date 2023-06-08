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

        public ForumThreadController(ILogger<ForumThreadController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] ForumThreadDTO dto)
        {
            var currentUser = Token.GetCurrentUser(HttpContext); // Get user from token rather than from the dto.

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
    }
}
