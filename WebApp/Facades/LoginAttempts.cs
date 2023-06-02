using System.Net;
using System.Threading;
using WebApp.ErrorHandling;

namespace WebApp.Facades
{
    public static class LoginAttempts
    {
        private static Dictionary<IPAddress, int> loginAttempts = new ();
        private static Dictionary<IPAddress, DateTime> login_timeouts = new ();
        private static Dictionary<IPAddress, string> captchas = new ();

        private static int MAX_ATTEMPTS = 10;
        private static int TIMEOUT_MINUTES = 1;
        
        public static async Task OnLoginAttempt(IPAddress IP)
        {
            int attempts;
            loginAttempts.TryGetValue(IP, out attempts);
            attempts++;
            loginAttempts.Remove(IP);
            loginAttempts.Add(IP, attempts);

            if(attempts < MAX_ATTEMPTS)
            {
                return;
            }

            DateTime timeout;
            bool hasTimeout = login_timeouts.TryGetValue(IP, out timeout);
            if(!hasTimeout)
            {
                timeout = DateTime.Now.AddMinutes(TIMEOUT_MINUTES);
                login_timeouts.Add(IP, timeout);
            }

            if (timeout > DateTime.Now)
            {
                await Task.Delay(5000);
                throw new API_Exception(HttpStatusCode.BadRequest, "Invalid login");
            }
            else
            {
                login_timeouts.Remove(IP);
                loginAttempts.Remove(IP);
            }
        }

        public static void OnSuccessfulLogin(IPAddress IP)
        {
            loginAttempts.Remove(IP);
            loginAttempts.Add(IP, 0);
        }
    }
}
