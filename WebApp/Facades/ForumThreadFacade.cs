using MySqlConnector;
using System.Data;
using System.Net;
using WebApp.Database;
using WebApp.DTOS;
using WebApp.ErrorHandling;
using WebApp.Model;
using Ganss.Xss;

namespace WebApp.Facades
{
    internal class ForumThreadFacade
    {
        private static HtmlSanitizer sanitizer = new HtmlSanitizer();

        private ILogger _logger;

        public ForumThreadFacade(ILogger logger)
        {
            _logger = logger;
        }


        internal void Create(ForumThread forumThread)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                MySqlTransaction transaction;
                transaction = connection.BeginTransaction();
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    string title_sanitized = sanitizer.Sanitize(forumThread.Title);
                    string content_sanitized = sanitizer.Sanitize(forumThread.Content);

                    //command.CommandText = $"Insert into db_forum_thread (title, content, user_id) VALUES ('{forumThread.Title}', '{forumThread.Content}', {forumThread.Author.Id})";

                    // Prepared statement query
                    command.CommandText = "Insert into db_forum_thread (title, content, user_id) VALUES (@title, @content, @userId)";
                    command.Parameters.AddWithValue("@title", title_sanitized);
                    command.Parameters.AddWithValue("@content", content_sanitized);
                    command.Parameters.AddWithValue("@userId", forumThread.Author.Id);
                    command.Prepare();
                    command.ExecuteNonQuery();

                    // Attempt to commit the transaction.
                    transaction.Commit();
                    _logger.LogInformation($"[{DateTime.Now}] Created user forum thread '{title_sanitized}'.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[{DateTime.Now}]" + ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError($"[{DateTime.Now}]" + ex.Message);
                    }

                    throw new API_Exception(HttpStatusCode.InternalServerError, "Internal server error");
                }
            }
        }

        internal async Task<ForumThread?> Get(long id)
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

                User? author = await UserFacade.Get(userId);
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

        internal async Task<List<ForumThreadDTO>> GetAll()
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_forum_thread";


                MySqlDataReader reader = command.ExecuteReader();
                List<ForumThreadDTO> list = new();
                while (reader.Read())
                {
                    long id = (long)reader["id"];
                    string title = reader["title"].ToString();
                    string content = reader["content"].ToString();
                    long userId = (long)reader["user_Id"];
                    User? user = await UserFacade.Get(userId);
                    var thread = new ForumThread()
                    {
                        Id = id,
                        Title = title,
                        Content = content,
                        Author = user
                    };

                    var dto = new ForumThreadDTO(thread);
                    list.Add(dto);
                }

                return list;
            }
        }
    }
}
