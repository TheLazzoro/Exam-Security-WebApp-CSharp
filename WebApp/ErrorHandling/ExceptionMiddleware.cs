using System.Net;

namespace WebApp.ErrorHandling
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (API_Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)ex.StatusCode;
                var error = new MessageDTO
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
                var error = new MessageDTO
                {
                    StatusCode = context.Response.StatusCode,
                    Message = message
                };

                await context.Response.WriteAsync(error.ToString());
            }
        }
    }
}
