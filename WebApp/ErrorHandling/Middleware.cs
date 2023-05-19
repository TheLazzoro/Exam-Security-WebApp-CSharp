using Microsoft.IdentityModel.Tokens;
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
        public Middleware(RequestDelegate next)
        {
            _next = next;
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
