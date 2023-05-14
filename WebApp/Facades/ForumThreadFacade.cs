using MySqlConnector;
using System.Data;
using System.Net;
using WebApp.Database;
using WebApp.ErrorHandling;
using WebApp.Model;

namespace WebApp.Facades
{
    internal static class ForumThreadFacade
    {
        internal static void Create(ForumThread forumThread)
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
                    command.CommandText = "Insert into db_forum_thread (title, content, user_id) VALUES (@title, @content, @userId)";
                    command.Parameters.AddWithValue("@title", forumThread.Title);
                    command.Parameters.AddWithValue("@content", forumThread.Content);
                    command.Parameters.AddWithValue("@userId", forumThread.Author.Id);
                    command.Prepare();
                    command.ExecuteNonQuery();

                    // Attempt to commit the transaction.
                    transaction.Commit();
                    Console.WriteLine($"Created user forum thread '{forumThread.Title}'.");
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

                    throw new API_Exception(HttpStatusCode.InternalServerError, "Internal server error");
                }
            }
        }

        internal static ForumThread? Get(long id)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_forum_thread where id = @id";
                command.Parameters.AddWithValue("@id", id);

                string? title = null;
                string? content = null;
                long userId = 0;
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    title = reader["title"].ToString();
                    content = reader["content"].ToString();
                    userId = (long)reader["user_Id"];
                }
                else
                {
                    throw new API_Exception(HttpStatusCode.NotFound, "");
                }

                User author = UserFacade.Get(userId);
                ForumThread forumThread = new ForumThread
                {
                    Id = id,
                    Title = title,
                    Content = content,
                    Author = author
                };

                return forumThread;
            }
        }
    }
}
