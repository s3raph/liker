using Dapper;
using Liker.Instagram;
using Microsoft.Data.Sqlite;

namespace Liker.Persistence
{
    internal class Database : IDatabase, IDisposable
    {
        public readonly string ConnectionString;

        private SqliteConnection? Connection;
        private bool _databaseInitialized = false;

        public Database(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public Database()
        {
            ConnectionString = "Data Source=Liker.db";
        }

        public async Task InsertFollowerAsync(AccountFollower toInsert)
        {
            var connection = await GetConnection();
            await EnsureDatabaseInitialized(connection);

            await connection.ExecuteAsync(
                "INSERT INTO AccountFollower VALUES (@userId, @userName, @following, @isPrivate, @isRestricted, @followerCount, @postsLiked, date());",
                new
                {
                    userId        = toInsert.UserID,
                    userName      = toInsert.Username,
                    following     = toInsert.Following,
                    isPrivate     = toInsert.IsPrivate,
                    isRestricted  = toInsert.IsRestricted,
                    followerCount = toInsert.FollowerCount,
                    postsLiked    = toInsert.PostsLiked
                });
        }

        public async Task<IReadOnlyCollection<AccountFollower>> GetFollowersAsync(params string[] followerPks)
        {
            var connection = await GetConnection();
            await EnsureDatabaseInitialized(connection);

            return (await Connection.QueryAsync<AccountFollower>($"SELECT * FROM AccountFollower WHERE UserID in ({string.Join(',', followerPks)})")).ToList();
        }

        public async Task<Account> GetAccountAsync(string accountUserName)
        {
            var connection = await GetConnection();
            await EnsureDatabaseInitialized(connection);

            return await Connection.QueryFirstOrDefaultAsync<Account>($"SELECT * FROM Account WHERE Username = @userName LIMIT 1", new { userName = accountUserName });
        }

        public async Task SetAccountNextMaxIdAsync(string accountUserName, string nextMaxId)
        {
            var connection = await GetConnection();
            await EnsureDatabaseInitialized(connection);

            await connection.ExecuteAsync(
                @"
                INSERT INTO Account(Username, NextMaxId) VALUES(@userName, @nextMaxId)
                  ON CONFLICT(Username) DO UPDATE SET NextMaxId = @nextMaxId
                ",
                new
                {
                    userName  = accountUserName,
                    nextMaxId = nextMaxId
                });
        }

        public async Task DeleteAccountAsync(string accountUserName)
        {
            var connection = await GetConnection();
            await EnsureDatabaseInitialized(connection);

            await connection.ExecuteAsync("DELETE FROM Account WHERE Username = @userName", new { userName = accountUserName });
        }

        private async Task<SqliteConnection> GetConnection()
        {
            if (Connection == null)
            {
                Connection = new SqliteConnection(ConnectionString);
                await Connection.OpenAsync();
            }

            return Connection;
        }

        private async Task EnsureDatabaseInitialized(SqliteConnection connection)
        {
            if (!_databaseInitialized)
            {
                if (!(await Connection.QueryAsync("SELECT name FROM sqlite_master WHERE type='table' AND name='AccountFollower'")).Any())
                {
                    var command = connection.CreateCommand();

                    command.CommandText =
                        @"
                        CREATE TABLE AccountFollower (
                            UserID        INTEGER      NOT NULL PRIMARY KEY,
                            Username      TEXT         NOT NULL,
                            Following     bit          NOT NULL,
                            IsPrivate     bit          NOT NULL,
                            IsRestricted  bit          NOT NULL,
                            FollowerCount INTEGER      NULLABLE,
                            PostsLiked    INTEGER      NULLABLE,
                            LastSeen      TEXT         NOT NULL
                        );
                        ";

                    await command.ExecuteNonQueryAsync();
                }

                if (!(await Connection.QueryAsync("SELECT name FROM sqlite_master WHERE type='table' AND name='Account'")).Any())
                {
                    var command = connection.CreateCommand();

                    command.CommandText =
                        @"
                        CREATE TABLE Account (
                            Username  TEXT NOT NULL PRIMARY KEY,
                            NextMaxId TEXT
                        );
                        ";

                    await command.ExecuteNonQueryAsync();
                }

                _databaseInitialized = true;
            }
        }

        private bool _disposed = false;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (Connection != null)
                {
                    Connection.Dispose();
                    Connection = null;
                }
            }
        }
    }
}
