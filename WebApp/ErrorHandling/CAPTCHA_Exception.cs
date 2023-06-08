using System.Net;

namespace WebApp.ErrorHandling
{
    public class CAPTCHA_Exception : API_Exception
    {
        public string captcha { get; set; }

        public CAPTCHA_Exception(string captcha_image) : base(HttpStatusCode.BadRequest, "Invalid Login")
        {
            captcha = captcha_image;
        }
    }
}
