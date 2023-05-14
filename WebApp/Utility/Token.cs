using System.Security.Claims;
using WebApp.Model;

namespace WebApp.Utility
{
    public static class Token
    {
        public static User GetCurrentUser(HttpContext context)
        {
            var identity = context.User.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return null;
            }

            var userClaims = identity.Claims;
            return new User
            {
                Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value,
                Id = long.Parse(userClaims.FirstOrDefault(o => o.Type == ClaimTypes.UserData)?.Value)
            };
        }
    }
}
