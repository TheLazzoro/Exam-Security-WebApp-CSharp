
using BCrypt.Net;
using WebApp.DTOS;

namespace WebApp.Model
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }

        /// <summary>
        /// Should only be used when fetching from database.
        /// </summary>
        public User() { }

        public User(string username, string password, Role role)
        {
            this.Username = username;
            this.Password = BCrypt.Net.BCrypt.HashPassword(password);
            this.Role = role;
        }

        public User(UserSafeDTO userDTO)
        {
            this.Username = userDTO.Username;
        }

        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, this.Password);
        }
    }
}
