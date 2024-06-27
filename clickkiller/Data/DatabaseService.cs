using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Data.Sqlite;
using clickkiller.Data.Migrations;

namespace clickkiller.Data
{
    public class Issue
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public required string Application { get; set; }
        public required string Notes { get; set; }
        public bool IsDone { get; set; }
    }

    public class DatabaseService
    {
        private readonly string ConnectionString;
        private const int CurrentVersion = 1;

        public DatabaseService(string appDataPath)
        {
            
            ConnectionString = $"Data Source={Path.Combine(appDataPath, "issues.db")}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            CreateVersionTableIfNotExists(connection);
            int currentVersion = GetCurrentVersion(connection);

            if (currentVersion < CurrentVersion)
            {
                ApplyMigrations(connection, currentVersion);
            }
        }

        private void CreateVersionTableIfNotExists(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS DatabaseVersion (
                    Version INTEGER NOT NULL
                )
            ";
            command.ExecuteNonQuery();
        }

        private int GetCurrentVersion(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Version FROM DatabaseVersion";
            var result = command.ExecuteScalar();

            if (result != null)
            {
                return Convert.ToInt32(result);
            }

            // If no version found, assume it's a new database
            command.CommandText = "INSERT INTO DatabaseVersion (Version) VALUES (0)";
            command.ExecuteNonQuery();
            return 0;
        }

        private void ApplyMigrations(SqliteConnection connection, int currentVersion)
        {
            var migrations = new List<IMigration>
            {
                new AddIsDoneColumnMigration()
            };

            foreach (var migration in migrations.OrderBy(m => m.Version))
            {
                if (migration.Version > currentVersion)
                {
                    migration.Up(connection);
                    UpdateDatabaseVersion(connection, migration.Version);
                }
            }
        }

        private void UpdateDatabaseVersion(SqliteConnection connection, int version)
        {
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE DatabaseVersion SET Version = $version";
            command.Parameters.AddWithValue("$version", version);
            command.ExecuteNonQuery();
        }

        public void SaveIssue(string application, string notes)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                INSERT INTO Issues (Timestamp, Application, Notes, IsDone)
                VALUES ($timestamp, $application, $notes, $isDone)
            ";
            command.Parameters.AddWithValue("$timestamp", DateTime.Now.ToString("o"));
            command.Parameters.AddWithValue("$application", application);
            command.Parameters.AddWithValue("$notes", notes);
            command.Parameters.AddWithValue("$isDone", 0);

            command.ExecuteNonQuery();
        }

        public List<Issue> GetAllIssues()
        {
            var issues = new List<Issue>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Timestamp, Application, Notes, IsDone FROM Issues ORDER BY Timestamp DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                issues.Add(new Issue
                {
                    Id = reader.GetInt32(0),
                    Timestamp = DateTime.Parse(reader.GetString(1)),
                    Application = reader.GetString(2),
                    Notes = reader.GetString(3),
                    IsDone = reader.GetInt32(4) != 0
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

        public void ToggleIssueDoneStatus(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Issues SET IsDone = NOT IsDone WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();
        }
    }
}
