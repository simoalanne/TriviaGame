namespace TriviaGame;
using Npgsql;

public static class DbSetup
{
    public static async Task EnsureTablesExistAsync(string connectionString)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        const string sql = "CREATE TABLE IF NOT EXISTS TriviaItems (Data jsonb NOT NULL, CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW());";
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}
