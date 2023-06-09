﻿using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using WebApp.DTOS;
using MySqlConnector;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using WebApp.Model;
using WebApp.Facades;
using System.Text.Json.Nodes;
using WebApp.ErrorHandling;
using WebApp.Utility;

namespace WebApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private IConfiguration _config;
        private readonly ILogger _logger;
        private static RequestLimiter requestLimiter = new RequestLimiter(10, 60); // 10 attempts, 60 second timeout.

        public LoginController(IConfiguration config, ILogger<LoginController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            bool validAttempt = await requestLimiter.OnRequest(userDTO.Username, HttpContext, _logger);
            if(!validAttempt)
            {
                return BadRequest();
            }
            await Task.Delay(1000);

            User? user = await LoginFacade.VerifyLogin(userDTO);
            requestLimiter.OnSuccessfulLogin(userDTO, HttpContext, _logger);


            var token = Token.GenerateToken(user);
            JsonObject jsonToken = new JsonObject();
            jsonToken.Add("token", token);
            jsonToken.Add("username", user.Username);

            return Ok(jsonToken);
        }

    }
}
