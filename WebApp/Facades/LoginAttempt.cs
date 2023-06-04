using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using WebApp.DTOS;
using WebApp.ErrorHandling;

namespace WebApp.Facades
{
    public class LoginAttempt
    {
        public IPAddress IP { get; }
        public string username { get; }
        public int attempts { get; set; }
        public DateTime timeout { get; set; }
        public bool hasTimeout { get; set; }


        private static ConcurrentDictionary<Tuple<IPAddress, string>, LoginAttempt> loginAttempts = new();

        private static int MAX_ATTEMPTS = 10;
        private static int TIMEOUT_MINUTES = 1;
        private static int LOGIN_DELAY = 1000 * 20; // 20 seconds

        private static int tmp_int;
        private static DateTime tmp_date;

        public LoginAttempt(IPAddress IP, string username)
        {
            this.IP = IP;
            this.username = username;
            this.attempts = 0;
            this.hasTimeout = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="API_Exception"></exception>
        public static async Task OnAttempt(UserDTO userDTO, HttpContext context, ILogger logger)
        {
            IPAddress IP = context.Connection.RemoteIpAddress;
            string username = userDTO.Username;
            var key = new Tuple<IPAddress, string>(IP, username);

            LoginAttempt loginAttempt;
            if (!loginAttempts.TryGetValue(key, out loginAttempt))
            {
                loginAttempt = new LoginAttempt(IP, username);
                loginAttempts.TryAdd(key, loginAttempt);
            }

            loginAttempt.attempts++;
            if (loginAttempt.attempts < MAX_ATTEMPTS)
            {
                return;
            }

            if (!loginAttempt.hasTimeout)
            {
                loginAttempt.timeout = DateTime.Now.AddMinutes(TIMEOUT_MINUTES);
                loginAttempt.hasTimeout = true;
            }

            if (loginAttempt.timeout > DateTime.Now)
            {
                logger.LogWarning($"Failed login attempt from IP '{IP}' with username '{userDTO.Username}'. Attempts: {loginAttempt.attempts}.");
                await Task.Delay(LOGIN_DELAY);

                // Refresh timeout
                loginAttempt.timeout = DateTime.Now.AddMinutes(TIMEOUT_MINUTES);

                throw new API_Exception(HttpStatusCode.BadRequest, "Invalid login");
            }
            else
            {
                loginAttempts.Remove(key, out loginAttempt);
            }
        }

        public static void OnSuccessfulLogin(UserDTO userDTO, HttpContext context)
        {
            LoginAttempt loginAttempt;
            var IP = context.Connection.RemoteIpAddress;
            string username = userDTO.Username;
            var key = new Tuple<IPAddress, string>(IP, username);

            loginAttempts.Remove(key, out loginAttempt);
        }
    }
}
