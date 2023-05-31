using Microsoft.AspNetCore.StaticFiles;
using MySqlConnector;
using System.Net;
using System.Text;
using WebApp.Database;
using WebApp.DTOS;
using WebApp.ErrorHandling;
using WebApp.Model;

namespace WebApp.Facades
{
    public static class LoginFacade
    {
        private static int MAX_ATTEMPTS = 3;
        private static readonly string CAPTCHA_STR = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly int CAPTCHA_LENGTH = 6;

        /// <summary>
        /// Returns null if user login was invalid
        /// </summary>
        /// <exception cref="API_Exception"></exception>
        public static async Task<User> VerifyLogin(UserDTO userDTO)
        {
            User? user = await UserFacade.Get(userDTO.Username);
            if (user == null)
            {
                throw new API_Exception(HttpStatusCode.Unauthorized, "Invalid login");
            }

            if (user.VerifyPassword(userDTO.Password))
            {
                // Reset login attempts
                using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
                {
                    await connection.OpenAsync();
                    MySqlCommand command = new MySqlCommand();
                    MySqlTransaction transaction = await connection.BeginTransactionAsync();
                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText = "update db_login_attempts set attempts = @attempts where username = @username";
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@attempts", 0);
                    try
                    {
                        await command.PrepareAsync();
                        await command.ExecuteNonQueryAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                    }
                }

                return user;
            }

            // if invalid login, add login attempts

            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                await connection.OpenAsync();
                MySqlCommand command = new MySqlCommand();
                command.Connection = connection;

                command.CommandText = "select attempts from db_login_attempts where username = @username";
                command.Parameters.AddWithValue("@username", userDTO.Username);
                await command.PrepareAsync();

                MySqlDataReader reader = await command.ExecuteReaderAsync();
                int attempts = 0;
                if (reader.Read())
                {
                    attempts = (int)reader["attempts"];
                    attempts++;
                    await reader.DisposeAsync(); // Need to close current reader before a new transaction.

                    MySqlCommand cmdIncrement = new MySqlCommand();
                    MySqlTransaction transaction = await connection.BeginTransactionAsync();
                    cmdIncrement.Connection = connection;
                    cmdIncrement.Transaction = transaction;
                    cmdIncrement.CommandText = "update db_login_attempts set attempts = @attempts where username = @username";
                    cmdIncrement.Parameters.AddWithValue("@attempts", attempts);
                    cmdIncrement.Parameters.AddWithValue("@username", user.Username);
                    try
                    {
                        await cmdIncrement.PrepareAsync();
                        await cmdIncrement.ExecuteNonQueryAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                    }

                    if (attempts > MAX_ATTEMPTS)
                    {
                        Random rand = new Random();
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < CAPTCHA_LENGTH; i++)
                        {
                            char c = CAPTCHA_STR[rand.Next(CAPTCHA_STR.Length)];
                            sb.Append(c);
                        }

                        string captcha = sb.ToString();
                        using (MemoryStream stream = CaptchaGen.NetCore.ImageFactory.BuildImage(captcha, 64, 256, 30, 10))
                        {
                            byte[] buffer = stream.ToArray();
                            string captcha_image = System.Convert.ToBase64String(buffer);

                            throw new API_Exception(HttpStatusCode.Unauthorized, captcha_image);
                        }
                    }

                }
                else // if user has not logged in even once.
                {
                    await reader.CloseAsync(); // Need to close current reader before a new transaction.
                    MySqlCommand commandInsert = new MySqlCommand();
                    MySqlTransaction transaction = await connection.BeginTransactionAsync();
                    commandInsert.Connection = connection;
                    commandInsert.Transaction = transaction;
                    commandInsert.CommandText = "insert into db_login_attempts (user_Id, username, attempts) values (@user_Id, @username, @attempts)";
                    commandInsert.Parameters.AddWithValue("@user_Id", user.Id);
                    commandInsert.Parameters.AddWithValue("@username", user.Username);
                    commandInsert.Parameters.AddWithValue("@attempts", 1);
                    try
                    {
                        await commandInsert.PrepareAsync();
                        await commandInsert.ExecuteNonQueryAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                    }
                }
            }

            return null;
        }
    }
}
