
using BCrypt.Net;

namespace Model
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        /// <summary>
        /// Should only be used when fetching from database.
        /// </summary>
        public User() { }

        public User(string username, string password, string role)
        {
            this.Username = username;
            this.Password = BCrypt.Net.BCrypt.HashPassword(password);
            this.Role = role;
        }

        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, this.Password);
        }
    }
}
