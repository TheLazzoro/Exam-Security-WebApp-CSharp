using Microsoft.AspNetCore.StaticFiles;
using MySqlConnector;
using SixLaborsCaptcha.Core;
using System.Net;
using System.Text;
using System.Web.Http.ModelBinding;
using WebApp.Database;
using WebApp.DTOS;
using WebApp.ErrorHandling;
using WebApp.Model;

namespace WebApp.Facades
{
    public static class LoginFacade
    {
        private static int MAX_ATTEMPTS = 3;
        private static readonly int CAPTCHA_LENGTH = 6;
#if _WINDOWS
        private static readonly string CAPTCHA_FONT = "Verdana";
#else
        private static readonly string CAPTCHA_FONT = "Lato";
#endif

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
                int loginAttempts = 0;
                if (reader.Read()) // if attempt count has been stored once, retreive it.
                {
                    loginAttempts = (int)reader["attempts"];
                    loginAttempts++;
                    await reader.DisposeAsync(); // Need to close current reader before a new transaction.

                    MySqlCommand cmdIncrement = new MySqlCommand();
                    MySqlTransaction transaction = await connection.BeginTransactionAsync();
                    cmdIncrement.Connection = connection;
                    cmdIncrement.Transaction = transaction;
                    cmdIncrement.CommandText = "update db_login_attempts set attempts = @attempts where username = @username";
                    cmdIncrement.Parameters.AddWithValue("@attempts", loginAttempts);
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
                }
                else // if user has not logged in even once.
                {
                    await reader.CloseAsync(); // Need to close current reader before a new transaction.
                    MySqlCommand commandInsert = new MySqlCommand();
                    MySqlTransaction transaction = await connection.BeginTransactionAsync();
                    commandInsert.Connection = connection;
                    commandInsert.Transaction = transaction;
                    commandInsert.CommandText = "insert into db_login_attempts (user_Id, username, attempts, captcha) values (@user_Id, @username, @attempts, @captcha)";
                    commandInsert.Parameters.AddWithValue("@user_Id", user.Id);
                    commandInsert.Parameters.AddWithValue("@username", user.Username);
                    commandInsert.Parameters.AddWithValue("@attempts", 1);
                    commandInsert.Parameters.AddWithValue("@captcha", string.Empty);
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

                // --- CAPTCHA CHECK --- //

                MySqlCommand cmd_GetCaptcha = new MySqlCommand();
                cmd_GetCaptcha.Connection = connection;
                cmd_GetCaptcha.CommandText = "select captcha from db_login_attempts where username = @username";
                cmd_GetCaptcha.Parameters.AddWithValue("@username", user.Username);
                await cmd_GetCaptcha.PrepareAsync();

                MySqlDataReader reader_captcha = await cmd_GetCaptcha.ExecuteReaderAsync();
                if (reader_captcha.Read())
                {
                    string? captcha = reader["captcha"].ToString();
                    if (captcha != userDTO.captcha)
                    {
                        throw new API_Exception(HttpStatusCode.Unauthorized, "Invalid login");
                    }
                }

                // --- Actually verify password and login --- //

                if (user.VerifyPassword(userDTO.Password))
                {
                    // Reset login attempts
                    await connection.OpenAsync();
                    MySqlCommand cmd_login = new MySqlCommand();
                    MySqlTransaction transaction = await connection.BeginTransactionAsync();
                    cmd_login.Connection = connection;
                    cmd_login.Transaction = transaction;

                    cmd_login.CommandText = "update db_login_attempts set attempts = @attempts where username = @username";
                    cmd_login.Parameters.AddWithValue("@username", user.Username);
                    cmd_login.Parameters.AddWithValue("@attempts", 0);
                    try
                    {
                        await cmd_login.PrepareAsync();
                        await cmd_login.ExecuteNonQueryAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                    }

                    return user;
                }


                // --- Generate CAPTCHA for user on too many login attempts --- //

                if (loginAttempts > MAX_ATTEMPTS)
                {
                    if (!string.IsNullOrEmpty(userDTO.captcha))
                    {

                    }

                    var slc = new SixLaborsCaptchaModule(new SixLaborsCaptchaOptions
                    {
                        DrawLines = 7,
                        TextColor = new Color[] { Color.Blue, Color.Black },
                        FontFamilies = new string[] { CAPTCHA_FONT },
                    });

                    string captcha = Extensions.GetUniqueKey(CAPTCHA_LENGTH);
                    byte[] buffer = slc.Generate(captcha);
                    string captcha_image = System.Convert.ToBase64String(buffer);

                    // Store CAPTCHA for user.

                    MySqlCommand cmd_store_captcha = new MySqlCommand();
                    MySqlTransaction trans_captcha = await connection.BeginTransactionAsync();
                    cmd_store_captcha.Connection = connection;
                    cmd_store_captcha.Transaction = trans_captcha;
                    cmd_store_captcha.CommandText = "update db_login_attempts set captcha = @captcha where username = @username";
                    cmd_store_captcha.Parameters.AddWithValue("@captcha", captcha);
                    cmd_store_captcha.Parameters.AddWithValue("@username", user.Username);
                    try
                    {
                        await cmd_store_captcha.PrepareAsync();
                        await cmd_store_captcha.ExecuteNonQueryAsync();
                        await trans_captcha.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await trans_captcha.RollbackAsync();
                    }


                    throw new API_Exception(HttpStatusCode.Unauthorized, captcha_image);
                }
            }

            return null;
        }
    }
}
