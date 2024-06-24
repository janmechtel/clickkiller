using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace clickkiller.Data
{
    public class Issue
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Application { get; set; }
        public string Notes { get; set; }
    }

    public class DatabaseService
    {
        private const string ConnectionString = "Data Source=issues.db";

        public DatabaseService()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Issues (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    Application TEXT NOT NULL,
                    Notes TEXT NOT NULL
                )
            ";
            command.ExecuteNonQuery();
        }

        public void SaveIssue(string application, string notes)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                INSERT INTO Issues (Timestamp, Application, Notes)
                VALUES ($timestamp, $application, $notes)
            ";
            command.Parameters.AddWithValue("$timestamp", DateTime.Now.ToString("o"));
            command.Parameters.AddWithValue("$application", application);
            command.Parameters.AddWithValue("$notes", notes);

            command.ExecuteNonQuery();
        }

        public List<Issue> GetAllIssues()
        {
            var issues = new List<Issue>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Timestamp, Application, Notes FROM Issues ORDER BY Timestamp DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                issues.Add(new Issue
                {
                    Id = reader.GetInt32(0),
                    Timestamp = DateTime.Parse(reader.GetString(1)),
                    Application = reader.GetString(2),
                    Notes = reader.GetString(3)
                });
            }

            return issues;
        }

        public void DeleteIssue(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Issues WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();
        }
    }
}
