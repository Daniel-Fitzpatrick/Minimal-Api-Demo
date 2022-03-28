using Dapper;

namespace Minimal.Api.Common.Data
{
    public class DatabaseInitializer
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DatabaseInitializer(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InitializeAsync()
        {
            using var connection = await _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                @"CREATE TABLE IF NOT EXISTS Users (
                        Id TEXT PRIMARY KEY,
                        Email TEXT NOT NULL,
                        Firstname TEXT NOT NULL,
                        Lastname TEXT NOT NULL,
                        DateOfBirth TEXT NOT NULL,
                        Skills TEXT NULL,
                        YearsOfExperience INTEGER NOT NULL
                    )"
                );
        }
    }
}
