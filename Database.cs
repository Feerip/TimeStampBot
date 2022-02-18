using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace _04_interactions_framework
{
    public class Database
    {
        //private static ulong testID = 197079662101397506;
        //private static string testTimeZone = "Pacific Standard Time";
        private readonly string _connectionString = "Data Source=mysql.db";
        SqliteConnection _connection;
        private readonly string _createDatabaseString = "CREATE TABLE IF NOT EXISTS discordUsers (id UINT64 NOT NULL PRIMARY KEY, timeZone VARCHAR(100))";


        SqliteCommand _createCommand;



        public Database()
        {
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
            _createCommand = new(_createDatabaseString, _connection);

            CreateTable();

            //InsertRow(testID, testTimeZone);

            //GetDiscordUserTimeZone();

        }
        ~Database()
        {
            _connection.Close();
        }

        public void CreateTable()
        {
            _createCommand.ExecuteNonQuery();
        }

        public void InsertRow(ulong id, string timeZone)
        {
            RemoveRow(id);

            var command = _connection.CreateCommand();
            command.CommandText =
            @"
                    INSERT INTO discordUsers (id, timeZone)
                    values ($id, $timeZone)
                ";
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("timeZone", timeZone);
            command.ExecuteNonQuery();
        }
        public string GetDiscordUserTimeZone(ulong id)
        {
            var command = _connection.CreateCommand();
            command.CommandText =
            @"
                    SELECT * 
                    FROM discordUsers
                    WHERE id = $id
                ";
            command.Parameters.AddWithValue("id", id);

            using (var reader = command.ExecuteReader())
            {
                string timeZone = "";
                while (reader.Read())
                {
                    timeZone = reader.GetString(1);
                    //Console.WriteLine($"{id} : {timeZone}");
                }
                if (!timeZone.Equals(""))
                    return timeZone;
                else
                    return null;
            }
        }

        public void RemoveRow(ulong id)
        {
            var command = _connection.CreateCommand();
            command.CommandText =
            @"
                    DELETE 
                    FROM discordUsers
                    WHERE id = $id
                ";
            command.Parameters.AddWithValue("id", id);

            command.ExecuteNonQuery ();
        }

    }
}
