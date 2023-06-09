﻿using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Principal;
using WebApp.Utility;

namespace WebApp.ErrorHandling
{
    public class Middleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public Middleware(RequestDelegate next, ILogger<Middleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// This method runs every time an endpoint is hit.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                /* TODO: Refresh access tokens.
                 * 
                var token = context.Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
                bool isVerified = Token.TokenVerify(token, context);
                if (!isVerified)
                {
                    token = Token.RefreshAccessToken(token);
                    context.Request.Headers[HeaderNames.Authorization] = token;
                }
                */

                await _next(context);
            }
            catch (CAPTCHA_Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)ex.StatusCode;
                var error = new ResponseDTO_CAPTCHA
                {
                    StatusCode = context.Response.StatusCode,
                    Message = ex.Message,
                    captcha = ex.captcha,
                };

                await context.Response.WriteAsync(error.ToString());
            }
            catch (API_Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)ex.StatusCode;
                var error = new ResponseDTO
                {
                    StatusCode = context.Response.StatusCode,
                    Message = ex.Message
                };

                await context.Response.WriteAsync(error.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Uncaught exception: {ex.Message}");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                string message;
                if(Globals.IsDevelopment)
                {
                    message = ex.Message;
                }
                else
                {
                    message = "Internal server error.";
                }
                var error = new ResponseDTO
                {
                    StatusCode = context.Response.StatusCode,
                    Message = message
                };

                await context.Response.WriteAsync(error.ToString());
            }
        }
    }
}
