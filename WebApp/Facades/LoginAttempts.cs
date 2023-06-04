using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using WebApp.DTOS;
using WebApp.ErrorHandling;

namespace WebApp.Facades
{
    public static class LoginAttempts
    {
        private static ConcurrentDictionary<IPAddress, int> loginAttempts = new ();
        private static ConcurrentDictionary<IPAddress, DateTime> login_timeouts = new ();
        private static ConcurrentDictionary<IPAddress, string> captchas = new ();

        private static int MAX_ATTEMPTS = 10;
        private static int TIMEOUT_MINUTES = 1;
        private static int LOGIN_DELAY = 1000 * 20; // 20 seconds

        private static int tmp_int;
        private static DateTime tmp_date;

        public static async Task OnLoginAttempt(UserDTO userDTO, HttpContext context, ILogger logger)
        {
            var IP = context.Connection.RemoteIpAddress;

            int attempts;
            loginAttempts.TryGetValue(IP, out attempts);
            attempts++;
            loginAttempts.Remove(IP, out tmp_int);
            loginAttempts.TryAdd(IP, attempts);

            if(attempts < MAX_ATTEMPTS)
            {
                return;
            }

            DateTime timeout;
            bool hasTimeout = login_timeouts.TryGetValue(IP, out timeout);
            if(!hasTimeout)
            {
                timeout = DateTime.Now.AddMinutes(TIMEOUT_MINUTES);
                login_timeouts.TryAdd(IP, timeout);
            }

            if (timeout > DateTime.Now)
            {
                logger.LogWarning($"Failed login attempts from IP '{IP}' with username '{userDTO.Username}'. Attempts: {attempts}");
                await Task.Delay(LOGIN_DELAY);
                
                // Refresh timeout
                timeout = DateTime.Now.AddMinutes(TIMEOUT_MINUTES);
                login_timeouts.Remove(IP, out tmp_date);
                login_timeouts.TryAdd(IP, timeout);

                throw new API_Exception(HttpStatusCode.BadRequest, "Invalid login");
            }
            else
            {
                login_timeouts.Remove(IP, out tmp_date);
                loginAttempts.Remove(IP, out tmp_int);
            }
        }

        public static void OnSuccessfulLogin(HttpContext context)
        {
            var IP = context.Connection.RemoteIpAddress;

            loginAttempts.Remove(IP, out tmp_int);
            loginAttempts.TryAdd(IP, 0);
        }
    }
}
