namespace WebApp.DTOS
{
    public struct UserDTO
    {
        public string Username { get; set; }
        public string? Password { get; set; }
        public string? captcha { get; set; }
    }
}
