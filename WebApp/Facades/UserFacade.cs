using Database;
using WebApp.DTOS;
using Model;
using MySqlConnector;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace Facades
{
    internal static class UserFacade
    {
        internal static void Create(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                MySqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction();

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    // Prepared statement query
                    command.CommandText = "Insert into db_user (username, passwd, user_role) VALUES (@username, @password, @role)";
                    MySqlParameter username = new MySqlParameter("@username", SqlDbType.VarChar);
                    MySqlParameter password = new MySqlParameter("@password", SqlDbType.VarChar);
                    MySqlParameter role = new MySqlParameter("@role", SqlDbType.VarChar);
                    username.Value = user.Username;
                    password.Value = user.Password;
                    role.Value = user.Role;
                    command.Parameters.Add(username);
                    command.Parameters.Add(password);
                    command.Parameters.Add(role);
                    command.Prepare();
                    command.ExecuteNonQuery();

                    // Attempt to commit the transaction.
                    transaction.Commit();
                    Console.WriteLine($"Created user '{user.Username}'.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
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
                MySqlParameter username_param = new MySqlParameter("@username", SqlDbType.VarChar);
                username_param.Value = username;
                command.Parameters.Add(username_param);

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

        internal static User? Get(long id)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_user where id = @id";
                MySqlParameter id_param = new MySqlParameter("@id", SqlDbType.BigInt);
                id_param.Value = id;
                command.Parameters.Add(id_param);

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
    }
}
