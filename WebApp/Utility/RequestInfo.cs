using System.Net;

namespace WebApp.Utility
{
    public class RequestInfo
    {
        public IPAddress IP { get; }
        public string username { get; }
        public int attempts { get; set; }
        public DateTime timeout { get; set; }
        public bool hasTimeout { get; set; }
        public Timer timer;

        public RequestInfo(IPAddress IP, string username)
        {
            this.IP = IP;
            this.username = username;
            attempts = 0;
            hasTimeout = false;
        }
    }
}
