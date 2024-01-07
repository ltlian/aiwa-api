using System.Runtime.CompilerServices;
using Lli.OpenAi.Core.Schema.Chat;
using Lli.OpenAi.Core.Schema.Thread;
using Microsoft.Data.Sqlite;

namespace AIWA.API.Data
{
    public class SqLiteStore(ILogger<SqLiteStore> logger) : IDataStore
    {
        private readonly string _connectionString = "Data Source=aiwa.db";
        private readonly ILogger<SqLiteStore> _logger = logger;

        public Task<List<IChatCompletionRequestMessage>> AddMessageToThreadAsync(int userId, string threadId, IChatCompletionRequestMessage message)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ThreadObject>> AddThreadToUserAsync(int userId, ThreadObject thread)
        {
            _logger.LogDebug("Adding thread to user {userId}", userId);

            string commandText = """
            INSERT INTO Threads(ThreadId, UserId, CreatedAt) VALUES ($threadId, $userId, $createdAt)
            """;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = commandText;

            var offset = DateTimeOffset.FromUnixTimeSeconds(thread.CreatedAt);
            command.Parameters.AddWithValue("$threadId", thread.Id);
            command.Parameters.AddWithValue("$createdAt", offset);
            command.Parameters.AddWithValue("$userId", userId);

            await command.ExecuteNonQueryAsync();

            return [];
        }

        public Task<ThreadObject> GetOrCreateUserThreadAsync(int userId, ThreadObject thread)
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<ThreadObject> GetUserThreadsAsync(int userId, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string commandText = """
            SELECT t.ThreadId, t.CreatedAt
            FROM Threads t
            INNER JOIN Users u ON t.UserId = u.UserId
            WHERE u.UserId = $userId
            """;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = commandText;

            command.Parameters.AddWithValue("$userId", userId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var thread = new ThreadObject
                (
                    reader.GetString(0),
                    "thread",
                    reader.GetInt64(1),
                    null
                );

                yield return thread;
            }
        }

        public async Task InitializeDatabaseAsync()
        {
            string commandText = """
            CREATE TABLE IF NOT EXISTS Users (
                UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                Username TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Threads (
                ThreadId TEXT PRIMARY KEY,
                UserId INTEGER,
                CreatedAt TIMESTAMP NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(UserId)
            );

            INSERT INTO Users(Username) VALUES ('Lars')
            """;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var createTableCommand = connection.CreateCommand();

            createTableCommand.CommandText = commandText;

            await createTableCommand.ExecuteNonQueryAsync();
        }
    }
}