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
using SixLabors.ImageSharp;

namespace WebApp.Facades
{
    internal static class UserFacade
    {


        /// <summary>
        /// Returns true if user was created.
        /// </summary>
        internal static async Task Create(UserDTO dto)
        {
            string password = dto.Password;
            if(string.IsNullOrEmpty(password)) {
                throw new API_Exception(HttpStatusCode.BadRequest, "Password cannot be empty.");
            }
            if(password.Length < 8) {
                throw new API_Exception(HttpStatusCode.BadRequest, "Password is too short.");
            }
            if(!password.Any(char.IsDigit) || !password.Any(char.IsLetter) || !password.Any(char.IsSymbol)) {
                throw new API_Exception(HttpStatusCode.BadRequest, "Password must contain at least one letter, number and symbol.");
            }

            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                MySqlTransaction transaction = connection.BeginTransaction();

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                var found = Get(dto.Username);
                if (found != null)
                {
                    throw new API_Exception(HttpStatusCode.Conflict, "Username was already taken.");
                }

                try
                {
                    Role role_user = await GetRoleByName("user");
                    User user = new User(dto.Username, dto.Password, role_user);

                    // Prepared statement query
                    command.CommandText = "Insert into db_user (username, passwd, role_Id) VALUES (@username, @password, @roleId)";
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@roleId", user.Role.Id);
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

        internal static async Task<User?> Get(string username)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_user where username = @username";
                command.Parameters.AddWithValue("@username", username);
                await command.PrepareAsync();

                long id = 0;
                string? password = null;
                long? roleId = null;
                MySqlDataReader reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    id = (long)reader["id"];
                    password = reader["passwd"].ToString();
                    roleId = (long)reader["role_Id"];
                }

                if (id == 0)
                    return null;

                Role role_user = await GetRoleById(roleId);
                User user = new User()
                {
                    Id = id,
                    Username = username,
                    Password = password,
                    Role = role_user,
                };

                return user;
            }
        }

        internal static async Task<User?> Get(long id)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_user where id = @id";
                command.Parameters.AddWithValue("@id", id);
                await command.PrepareAsync();

                string? username = null;
                string? password = null;
                long? roleId = null;
                MySqlDataReader reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    username = reader["username"].ToString();
                    password = reader["passwd"].ToString();
                    roleId = (long)reader["role_Id"];
                }

                if (username == null)
                    return null;

                Role role = await GetRoleById(roleId);
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

        internal static async Task<Role?> GetRoleById(long? id)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_role where id = @id";
                command.Parameters.AddWithValue("@id", id);
                await command.PrepareAsync();

                Role? role = null;
                MySqlDataReader reader = await command.ExecuteReaderAsync();
                if(reader.Read())
                {
                    string rolename = reader["rolename"].ToString();
                    role = new Role()
                    {
                        Id = id,
                        roleName = rolename
                    };
                }

                return role;
            }
        }

        internal static async Task<Role?> GetRoleByName(string name)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_role where rolename = @name";
                command.Parameters.AddWithValue("@name", name);
                await command.PrepareAsync();

                Role? role = null;
                MySqlDataReader reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    long id = (long)reader["id"];
                    role = new Role()
                    {
                        Id = id,
                        roleName = name
                    };
                }

                return role;
            }
        }

        internal static async Task UploadImage(IFormFile file, HttpContext context)
        {
            string filename = file.FileName;
            if(!filename.EndsWith(".jpg") && !filename.EndsWith(".png")) {
                throw new API_Exception(HttpStatusCode.BadRequest, "Invalid file type.");
            }

            byte[] buffer = new byte[file.Length];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                Stream s = file.OpenReadStream();
                while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
            }

            bool isJpeg;
            bool isPng;

            // Analyse file
            using (Stream s = new MemoryStream(buffer))
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

            // Extra check that parses the file content.
            // Throws exception if content is invalid.
            Image img = Image.Load(buffer);

            // File is valid/safe, we can continue.

            var user = Token.GetCurrentUser(context);

            string localDir = Globals.LocalImageDir;
            string username = user.Username;
            string dir = Path.Combine(localDir, username);

            // re-construct file name
            string fileName = $@"{System.Guid.NewGuid()}";
            string ext = string.Empty;

            if (isJpeg)
                ext = ".jpg";
            else if (isPng)
                ext = ".png";

            fileName += ext;
            string fullNewPath = Path.Combine(dir, fileName);

            if(!Directory.Exists(localDir))
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
                        var s = new MemoryStream(buffer);
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

        /// <summary>
        /// Slight overhead on this method, since we need to find the user by username first.
        /// </summary>
        internal static string GetUserImagePath(string username, HttpContext context)
        {
            var user = Get(username);
            return GetUserImagePath(user.Id, context);
        }

        internal static string GetUserImagePath(long userId, HttpContext context)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select username, user_image from db_user where id = @id";
                command.Parameters.AddWithValue("@id", userId);
                command.Prepare();

                string imagePath = string.Empty;
                string username = string.Empty;
                MySqlDataReader reader = command.ExecuteReader();
                if(reader.Read())
                {
                    imagePath = reader["user_image"].ToString();
                    username = reader["username"].ToString();
                }

                if (string.IsNullOrEmpty(imagePath))
                {
                    return Path.Combine(Directory.GetCurrentDirectory(), "Images/Default.png");
                }

                string host = $"{context.Request.Scheme}://{context.Request.Host}";
                string filepath = "/Images/" + username + "/" + Path.GetFileName(imagePath);

                return host + filepath;
            }
        }
    }
}
