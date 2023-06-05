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
    internal static class ForumThreadPostFacade
    {
        private static HtmlSanitizer sanitizer = new HtmlSanitizer();

        internal static async Task Create(ForumThreadPost forumThreadPost)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                MySqlTransaction transaction = connection.BeginTransaction();
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    string content_sanitized = sanitizer.Sanitize(forumThreadPost.Content);

                    command.CommandText = "Insert into db_forum_thread_post (content, user_id, forum_thread_Id) VALUES (@content, @userId, @forumThreadId)";
                    command.Parameters.AddWithValue("@content", content_sanitized);
                    command.Parameters.AddWithValue("@userId", forumThreadPost.Author.Id);
                    command.Parameters.AddWithValue("@forumThreadId", forumThreadPost.ThreadId);
                    await command.PrepareAsync();
                    await command.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                    Console.WriteLine($"Created user forum thread post.");
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
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }

                    throw new API_Exception(HttpStatusCode.InternalServerError, "Internal server error");
                }
            }
        }

        internal static async Task<ForumThreadPost?> Get(long? id)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_forum_thread_post where id = @id";
                command.Parameters.AddWithValue("@id", id);
                await command.PrepareAsync();

                string? content = null;
                long userId = 0;
                long threadId = 0;
                MySqlDataReader reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    content = reader["content"].ToString();
                    userId = (long)reader["user_Id"];
                    threadId = (long)reader["forum_thread_Id"];
                }
                else
                {
                    throw new API_Exception(HttpStatusCode.NotFound, "Forum thread post does not exist.");
                }

                User? author = await UserFacade.Get(userId);
                ForumThread? thread = await ForumThreadFacade.Get(threadId);
                ForumThreadPost forumThreadPost = new ForumThreadPost
                {
                    Id = id,
                    Content = content,
                    Author = author,
                    ThreadId = (long)thread.Id
                };

                return forumThreadPost;
            }
        }

        internal static async Task<List<ForumThreadPostDTO>?> GetByThreadId(long id)
        {
            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from db_forum_thread_post where forum_thread_Id = @id";
                command.Parameters.AddWithValue("@id", id);
                await command.PrepareAsync();

                string? title = null;
                string? content = null;
                long userId = 0;
                long threadId = 0;
                MySqlDataReader reader = await command.ExecuteReaderAsync();
                List<ForumThreadPostDTO> list = new List<ForumThreadPostDTO>();
                while (reader.Read())
                {
                    long postId = (long)reader["id"];
                    content = reader["content"].ToString();
                    userId = (long)reader["user_Id"];
                    threadId = (long)reader["forum_thread_Id"];

                    User? author = await UserFacade.Get(userId);
                    ForumThread? thread = await ForumThreadFacade.Get(threadId);
                    ForumThreadPost forumThreadPost = new ForumThreadPost
                    {
                        Id = postId,
                        Content = content,
                        Author = author,
                        ThreadId = (long)thread.Id
                    };

                    ForumThreadPostDTO dto = new ForumThreadPostDTO(forumThreadPost);
                    list.Add(dto);
                }

                return list;
            }
        }

        internal static async Task Edit(User user, ForumThreadPostDTO threadPostDTO)
        {
            long? id = threadPostDTO.Id;
            string content = threadPostDTO.Content;
            ForumThreadPost? post = await Get(id);
            if (post.Author.Id != user.Id)
            {
                throw new API_Exception(HttpStatusCode.Unauthorized, "Cannot edit another user's post.");
            }

            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                MySqlTransaction transaction = await connection.BeginTransactionAsync();
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "update db_forum_thread_post set content = @content where id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@content", content);
                    await command.PrepareAsync();
                    await command.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }

                    throw new API_Exception(HttpStatusCode.InternalServerError, "Internal server error");
                }
            }
        }

        internal static async Task Delete(User user, long id)
        {
            ForumThreadPost? post = await Get(id);
            if (post.Author.Id != user.Id)
            {
                throw new API_Exception(HttpStatusCode.Unauthorized, "Cannot delete another user's post.");
            }

            using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString))
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                MySqlTransaction transaction = await connection.BeginTransactionAsync();
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "delete from db_forum_thread_post where id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    await command.PrepareAsync();
                    await command.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }

                    throw new API_Exception(HttpStatusCode.InternalServerError, "Internal server error");
                }
            }
        }
    }
}
