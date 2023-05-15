﻿using Microsoft.AspNetCore.Authorization;
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
            ForumThread thread = new ForumThread(dto.Thread);

            ForumThreadPost forumThreadPost = new ForumThreadPost(dto.Content, currentUser, thread);
            ForumThreadPostFacade.Create(forumThreadPost);

            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            var forumThreadPost = ForumThreadPostFacade.Get(id);
            var dto = new ForumThreadPostDTO(forumThreadPost);
            return Ok(dto);
        }
    }
}