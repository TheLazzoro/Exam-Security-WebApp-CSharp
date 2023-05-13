using MySqlConnector;
using System.Data.SqlClient;
using System.Data;
using Database;

public static class Startup
{
    private static bool didStart = false;

    public static void Run()
    {
        if (didStart)
            return;

        didStart = true;

        using (MySqlConnection connection = new MySqlConnection(SQLConnection.connectionString_Startup))
        {
            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();

                string file = Path.Combine(Directory.GetCurrentDirectory(), "db.sql");
                string script = File.ReadAllText(file);

                // Prepared statement query
                command.CommandText = script;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
