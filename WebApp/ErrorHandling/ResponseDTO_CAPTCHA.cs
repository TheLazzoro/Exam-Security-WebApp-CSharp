using Newtonsoft.Json;

namespace WebApp.ErrorHandling
{
    public struct ResponseDTO_CAPTCHA
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? captcha { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
