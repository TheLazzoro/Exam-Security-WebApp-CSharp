namespace WebApp.DTOS
{
    public class UserSafeDTO
    {
        public string Username { get; set; }

        public UserSafeDTO(string username)
        {
            this.Username = username;
        }
    }
}
