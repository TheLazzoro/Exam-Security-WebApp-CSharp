using Database;
using WebApp.DTOS;
using Model;
using MySqlConnector;
using System.Data;
using System.Data.SqlClient;

namespace Facades
{
    public static class UserFacade
    {
        public static void Create(User user)
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
                    command.CommandText = "Insert into db_user (username, passwd) VALUES (@username, @password)";
                    MySqlParameter username = new MySqlParameter("@username", SqlDbType.VarChar);
                    MySqlParameter password = new MySqlParameter("@password", SqlDbType.VarChar);
                    username.Value = user.Username;
                    password.Value = user.Password;
                    command.Parameters.Add(username);
                    command.Parameters.Add(password);
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
    }
}
