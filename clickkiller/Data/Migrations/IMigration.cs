using Microsoft.Data.Sqlite;

namespace clickkiller.Data.Migrations
{
    public interface IMigration
    {
        int Version { get; }
        void Up(SqliteConnection connection);
        void Down(SqliteConnection connection);
    }
}
