using WebApp.DTOS;
using MySqlConnector;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Net;
using Microsoft.Extensions.FileProviders.Composite;
using System.Web.Http;
using WebApp.ErrorHandling;
using WebApp.Model;
using WebApp.Database;
using WebApp.Utility;
using System.Transactions;

namespace WebApp.Facades
{
    internal static class UserFacade
    {
        /// <summary>
        /// Returns true if user was created.
        /// </summary>
        internal static async Task Create(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                MySqlTransaction transaction = connection.BeginTransaction();

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                var found = Get(user.Username);
                if (found != null)
                {
                    throw new API_Exception(HttpStatusCode.Conflict, "Username was already taken.");
                }

                try
                {
                    // Prepared statement query
                    command.CommandText = "Insert into db_user (username, passwd, user_role) VALUES (@username, @password, @role)";
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@role", user.Role);
                    await command.PrepareAsync();
                    await command.ExecuteNonQueryAsync();

                    // Attempt to commit the transaction.
                    await transaction.CommitAsync();
                    Console.WriteLine($"Created user '{user.Username}'.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }

                    throw new API_Exception(HttpStatusCode.InternalServerError, "Internal server error.");
                }
            }
        }

        internal static User? Get(string username)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_user where username = @username";
                command.Parameters.AddWithValue("@username", username);
                command.Prepare();

                long id = 0;
                string? password = null;
                string? role = null;
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    id = (long)reader["id"];
                    password = reader["passwd"].ToString();
                    role = reader["user_role"].ToString();
                }

                if (id == 0)
                    return null;

                User user = new User()
                {
                    Id = id,
                    Username = username,
                    Password = password,
                    Role = role,
                };

                return user;
            }
        }

        internal static User? Get(long id)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_user where id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.Prepare();

                string? username = null;
                string? password = null;
                string? role = null;
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    username = reader["username"].ToString();
                    password = reader["passwd"].ToString();
                    role = reader["user_role"].ToString();
                }

                if (username == null)
                    return null;

                User user = new User()
                {
                    Id = id,
                    Username = username,
                    Password = password,
                    Role = role,
                };

                return user;
            }
        }

        internal static async Task UploadImage(byte[] file, HttpContext context)
        {
            bool isJpeg;
            bool isPng;

            // Analyse file
            using (Stream s = new MemoryStream(file))
            {
                BinaryReader reader = new BinaryReader(s);

                // JFIF markers
                UInt16 soi = reader.ReadUInt16();    // Start of Image (SOI) marker (FFD8)
                UInt16 marker = reader.ReadUInt16(); // JFIF marker (FFE0) or EXIF marker(FFE1)

                // PNG markers
                reader.BaseStream.Position = 0; // reset read position.
                UInt64 pngHeader = reader.ReadUInt64(); // PNG header (89 50 4E 47 0D 0A 1A 0A)

                isJpeg = soi == 0xd8ff && (marker & 0xe0ff) == 0xe0ff;
                isPng = pngHeader == 0x0a1a0a0d474e5089;

                bool isFileValid = isJpeg || isPng;
                if (!isFileValid)
                {
                    throw new API_Exception(HttpStatusCode.BadRequest, "Invalid file type.");
                }
            }

            // File is valid/safe, we can continue.

            var user = Token.GetCurrentUser(context);

            string specialFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Images");
            string username = user.Username;
            string dir = Path.Combine(specialFolder, username);

            // re-construct file name
            string fileName = $@"{System.Guid.NewGuid()}";
            string ext = string.Empty;

            if (isJpeg)
                ext = ".jpg";
            else if (isPng)
                ext = ".png";

            fileName += ext;
            string fullNewPath = Path.Combine(dir, fileName);

            if(!Directory.Exists(specialFolder))
            {
                // TODO:
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }


            // Before we write file,
            // we need to update the file path for the user.

            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command_GetImage = connection.CreateCommand();
                MySqlTransaction transaction = connection.BeginTransaction();

                command_GetImage.Connection = connection;
                command_GetImage.Transaction = transaction;

                try
                {
                    command_GetImage.CommandText = "select user_image from db_user where id = @id";
                    command_GetImage.Parameters.AddWithValue("@id", user.Id);
                    await command_GetImage.PrepareAsync();

                    string? oldImagePath = null;
                    MySqlDataReader reader = await command_GetImage.ExecuteReaderAsync();
                    if (reader.Read())
                    {
                        oldImagePath = reader["user_image"].ToString();
                    }
                    await reader.CloseAsync();

                    MySqlCommand command_UpdateImage = connection.CreateCommand();
                    command_UpdateImage.Connection = connection;
                    command_UpdateImage.Transaction = transaction;

                    command_UpdateImage.CommandText = "update db_user set user_image = @imagePath where id = @id;";
                    command_UpdateImage.Parameters.AddWithValue("@imagePath", fullNewPath);
                    command_UpdateImage.Parameters.AddWithValue("@id", user.Id);
                    await command_UpdateImage.PrepareAsync();
                    await command_UpdateImage.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();

                    // Delete old image
                    if (oldImagePath != null && File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }

                    // Write new image
                    using (FileStream fileStream = File.Create(fullNewPath))
                    {
                        var s = new MemoryStream(file);
                        s.CopyTo(fileStream);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }

                    throw new API_Exception(HttpStatusCode.InternalServerError, "Internal server error.");
                }
            }
        }

        internal static string GetUserImagePath(long userId)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select user_image from db_user where id = @id";
                command.Parameters.AddWithValue("@id", userId);
                command.Prepare();

                string imagePath = string.Empty;
                MySqlDataReader reader = command.ExecuteReader();
                if(reader.Read())
                {
                    imagePath = reader["user_image"].ToString();
                }

                if(string.IsNullOrEmpty(imagePath))
                {
                    imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images/Default.png");
                }

                return imagePath;
            }
        }
    }
}
