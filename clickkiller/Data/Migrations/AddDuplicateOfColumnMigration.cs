using Microsoft.Data.Sqlite;

namespace clickkiller.Data.Migrations
{
    public class AddDuplicateOfColumnMigration : IMigration
    {
        public int Version => 2;

        public void Up(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                ALTER TABLE Issues
                ADD COLUMN DuplicateOf INTEGER NULL
                REFERENCES Issues(Id)
            ";
            command.ExecuteNonQuery();
        }

        public void Down(SqliteConnection connection)
        {
            // SQLite doesn't support dropping columns, so we need to recreate the table
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                BEGIN TRANSACTION;

                CREATE TABLE Issues_temp (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    Application TEXT NOT NULL,
                    Notes TEXT NOT NULL,
                    IsDone INTEGER NOT NULL
                );

                INSERT INTO Issues_temp (Id, Timestamp, Application, Notes, IsDone)
                SELECT Id, Timestamp, Application, Notes, IsDone FROM Issues;

                DROP TABLE Issues;

                ALTER TABLE Issues_temp RENAME TO Issues;

                COMMIT;
            ";
            command.ExecuteNonQuery();
        }
    }
}
