using Dapper;
using Nero.Core.Entities;
using Npgsql;

namespace Nero.Core.Services
{
    public class UserService(IConfiguration configuration) : IUserService
    {

        private readonly string GET_USER_BY_ID = @"SELECT u.id, u.username FROM nero.user u WHERE u.id = @id";
        private readonly string connString = configuration["Database:ConnectionString"] ?? throw new Exception("Database connection string not found");
        public async Task<User?> GetBy(string userId)
        {
            using var connection = await GetConnection();
            var user = await connection.QueryFirstOrDefaultAsync<User>(GET_USER_BY_ID, new { id = userId });
            return user;
        }

         private async Task<NpgsqlConnection> GetConnection()
        {
            var connection = new NpgsqlConnection(connString);
            await connection.OpenAsync();
            return connection;
        }
    }
}