using WebApp.Model;

namespace WebApp.DTOS
{
    public class UserSafeDTO
    {
        public string Username { get; set; }
        public string? Role { get; set; }

        public UserSafeDTO()
        {
        }

        public UserSafeDTO(string Username, string Role)
        {
            this.Username = Username;
            this.Role = Role;
        }

        public UserSafeDTO(User user)
        {
            this.Username = user.Username;
            this.Role = user.Role;
        }
    }
}
