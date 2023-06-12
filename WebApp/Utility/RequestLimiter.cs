using System.Collections.Concurrent;
using System.Net;
using WebApp.DTOS;
using WebApp.ErrorHandling;

namespace WebApp.Utility
{
    public class RequestLimiter
    {
        private ConcurrentDictionary<Tuple<IPAddress, string>, RequestInfo> requests = new();

        private uint MAX_ATTEMPTS;
        private uint TIMEOUT_SECONDS;
        private uint TIMEOUT_MS;

        /// <summary>
        /// An instance of a request limiter, keeping track of IP-username pairs.
        /// NOTE: Must be declared static when used in controllers.
        /// </summary>
        public RequestLimiter(uint MAX_ATTEMPTS, uint TIMEOUT_SECONDS)
        {
            this.MAX_ATTEMPTS = MAX_ATTEMPTS;
            this.TIMEOUT_SECONDS = TIMEOUT_SECONDS;
            this.TIMEOUT_MS = TIMEOUT_SECONDS * 1000;
        }

        /// <summary>
        /// Returns true when current request attempt is lower than max attemtps.
        /// </summary>
        /// <exception cref="API_Exception"></exception>
        public async Task<bool> OnRequest(string username, HttpContext context, ILogger logger)
        {
            IPAddress IP = context.Connection.RemoteIpAddress;
            var key = new Tuple<IPAddress, string>(IP, username);

            RequestInfo requestInfo;
            if (!requests.TryGetValue(key, out requestInfo))
            {
                requestInfo = new RequestInfo(IP, username);
                if (requests.TryAdd(key, requestInfo))
                {
                    // object was successfully added, and we can start the timer.
                    TimerStart(requestInfo);
                }
            }

            requestInfo.attempts++;
            if (requestInfo.attempts < MAX_ATTEMPTS)
            {
                return true;
            }

            if (!requestInfo.hasTimeout)
            {
                requestInfo.timeout = DateTime.Now.AddSeconds(TIMEOUT_SECONDS);
                requestInfo.hasTimeout = true;
            }

            if (requestInfo.timeout > DateTime.Now)
            {
                logger.LogWarning($"[{DateTime.Now}]  Blocked request from IP '{IP}' with username '{username}'. Attempts: {requestInfo.attempts}.");
                //await Task.Delay(LOGIN_DELAY);

                // Refresh timeout
                requestInfo.timeout = DateTime.Now.AddSeconds(TIMEOUT_SECONDS);

                return false;
                //throw new API_Exception(HttpStatusCode.BadRequest, "Invalid login");
            }
            else
            {
                requests.Remove(key, out requestInfo);
                return true;
            }
        }

        public void OnSuccessfulLogin(UserDTO userDTO, HttpContext context, ILogger logger)
        {
            RequestInfo requestInfo;
            var IP = context.Connection.RemoteIpAddress;
            string username = userDTO.Username;
            var key = new Tuple<IPAddress, string>(IP, username);

            requests.Remove(key, out requestInfo);

            logger.LogInformation($"[{DateTime.Now}]  Successful login from IP '{IP}' with username '{userDTO.Username}'.");
        }


        private void TimerStart(RequestInfo requestInfo)
        {
            requestInfo.timer = new Timer(OnTimerFinish, requestInfo, TIMEOUT_MS, Timeout.Infinite);
        }

        /// <summary>
        /// Removes login attempt from memory after a delay.
        /// </summary>
        private void OnTimerFinish(object stateinfo)
        {
            var requestInfo = (RequestInfo)stateinfo;
            var IP = requestInfo.IP;
            var username = requestInfo.username;

            // Remove object from ConcurrentDictionary after 'TIMEOUT_MS' delay
            if (requestInfo.timeout < DateTime.Now)
            {
                var key = new Tuple<IPAddress, string>(IP, username);
                requests.Remove(key, out requestInfo);
            }
            else
            {
                // Run timer again if timeout is still active
                TimerStart(requestInfo);
            }
        }

    }
}
