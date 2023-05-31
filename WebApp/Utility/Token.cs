using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Transactions;
using WebApp.Database;
using WebApp.ErrorHandling;
using WebApp.Facades;
using WebApp.Model;

namespace WebApp.Utility
{
    public static class Token
    {
        public static TokenValidationParameters tokenValidationParameters;
        private static JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        private static readonly int TokenLifetimeHours = 2;

        public static string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(SharedSecret.GetSharedKey());
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Role, user.Role.roleName),
                new Claim("id", user.Id.ToString()),
            };

            var token = new JwtSecurityToken(
                null, //_config["Jwt:Issuer"],
                null, //_config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(TokenLifetimeHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static User GetCurrentUser(HttpContext context)
        {
            var identity = context.User.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return null;
            }

            var userClaims = identity.Claims;
            return new User
            {
                Username = userClaims.FirstOrDefault(o => o.Type == "username")?.Value,
                Role = new Role(userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value),
                Id = long.Parse(userClaims.FirstOrDefault(o => o.Type == "id")?.Value)
            };
        }

        internal static TokenValidationParameters GetValidationParameters()
        {
            if (tokenValidationParameters == null)
            {
                tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidIssuer = config["Jwt:Issuer"],
                    //ValidAudience = config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(SharedSecret.GetSharedKey()),
                };
            }

            return tokenValidationParameters;
        }

        public static bool TokenVerify(string token, HttpContext context)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var validationParameters = Token.GetValidationParameters();
            SecurityToken validatedToken;
            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (Exception) // token could not be verified.
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a new access token if a valid refresh token was found.
        /// If a token from a previous session matches the token being passed in,
        /// returns a new, fresh token.
        /// Otherwise, returns null.
        /// </summary>
        public static async Task<string?> RefreshAccessToken(string tokenStr)
        {
            if (string.IsNullOrEmpty(tokenStr))
            {
                return null;
            }

            var token = tokenHandler.ReadJwtToken(tokenStr);
            long userId = long.Parse((string)token.Payload["id"]);
            if (userId <= 0)
            {
                return null;
            }

            User user = await UserFacade.Get(userId);
            if (user == null)
            {
                return null;
            }

            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_refresh_token where token = @token";
                command.Parameters.AddWithValue("@token", tokenStr);
                command.Prepare();

                string? refreshedToken = tokenStr;
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string? storedToken = reader["token"].ToString();

                    if (tokenStr == storedToken)
                    {
                        refreshedToken = GenerateToken(user);
                    }

                    MySqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        MySqlCommand cmd_persist = new MySqlCommand();
                        cmd_persist.Connection = connection;
                        cmd_persist.Transaction = transaction;

                        cmd_persist.CommandText = "insert into db_refresh_token (userId, token) values (@userId, @token);";
                        cmd_persist.Parameters.AddWithValue("@userId", user.Id);
                        cmd_persist.Parameters.AddWithValue("@token", refreshedToken);
                        cmd_persist.Prepare();
                        cmd_persist.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                        Console.WriteLine("  Message: {0}", ex.Message);

                        try
                        {
                            transaction.RollbackAsync();
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                            Console.WriteLine("  Message: {0}", ex2.Message);
                        }

                        throw new API_Exception(HttpStatusCode.InternalServerError, "Internal server error.");
                    }
                }

                return refreshedToken;
            }
        }
    }
}
