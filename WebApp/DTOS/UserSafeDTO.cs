namespace WebApp.DTOS
{
    public struct UserSafeDTO
    {
        public string Username { get; set; }

        public UserSafeDTO(string username)
        {
            this.Username = username;
        }
    }
}
