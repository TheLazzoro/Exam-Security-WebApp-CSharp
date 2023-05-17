using WebApp.Model;

namespace WebApp.DTOS
{
    public class UserSafeDTO
    {
        public long Id { get; set; }
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
            this.Id = user.Id;
            this.Username = user.Username;
            this.Role = user.Role;
        }
    }
}
