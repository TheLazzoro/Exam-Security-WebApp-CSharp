using MySqlConnector;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Database
{
    public static class SQLConnection
    {
        public static string? connectionString;
        public static string? connectionString_Startup;
    }
}
