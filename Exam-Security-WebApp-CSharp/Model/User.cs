
namespace Model
{
    public class User
    {
        public long Id { get; }
        public string Username { get; set; }
        public string Password { get; set; }

        public User(string username, string password)
        {
            this.Username = username;
            this.Password = BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password)
        {
            password = BCrypt.Net.BCrypt.HashPassword(password);
            return password == this.Password;
        }
    }
}
