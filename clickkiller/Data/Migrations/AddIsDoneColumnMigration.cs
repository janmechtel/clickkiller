using Microsoft.Data.Sqlite;

namespace clickkiller.Data.Migrations
{
    public class AddIsDoneColumnMigration : IMigration
    {
        public int Version => 1;

        public void Up(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Issues (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    Application TEXT NOT NULL,
                    Notes TEXT NOT NULL
                );

                ALTER TABLE Issues ADD COLUMN IsDone INTEGER NOT NULL DEFAULT 0;
            ";
            command.ExecuteNonQuery();
        }

        public void Down(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE Issues_Temp (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    Application TEXT NOT NULL,
                    Notes TEXT NOT NULL
                );

                INSERT INTO Issues_Temp (Id, Timestamp, Application, Notes)
                SELECT Id, Timestamp, Application, Notes FROM Issues;

                DROP TABLE Issues;

                ALTER TABLE Issues_Temp RENAME TO Issues;
            ";
            command.ExecuteNonQuery();
        }
    }
}
