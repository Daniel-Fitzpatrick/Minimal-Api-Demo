using Dapper;
using Minimal.Api.Common.Data;
using Minimal.Api.Common.Model;

namespace Minimal.Api.Common.Services;

public interface IUserService
{
    Task<bool> CreateAsync(User user);

    Task<User?> GetByEmailAsync(string emailAddress);

    Task<User?> GetByIdAsync(Guid id);

    Task<IEnumerable<User>> GetAllAsync();

    Task<IEnumerable<User>> SearchBySkill(string skill);

    Task<bool> UpdateAsync(Guid id, User user);

    Task<bool> DeleteAsync(Guid id);

}

public class UserService : IUserService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(User user)
    {
        using var connection = await _connectionFactory.CreateConnection();

        var result = await connection.ExecuteAsync(@"INSERT INTO Users (
                                                                Id,
                                                                Email, 
                                                                FirstName,
                                                                LastName, 
                                                                Skills,
                                                                DateOfBirth,
                                                                YearsOfExperience
                                                             ) 
                                                             VALUES 
                                                             (
                                                                @Id, 
                                                                @Email,
                                                                @FirstName, 
                                                                @LastName, 
                                                                @Skills,
                                                                @DateOfBirth,
                                                                @YearsOfExperience
                                                             )", user);

        return result > 0;
    }

    public async Task<User?> GetByEmailAsync(string emailAddress)
    {
        using var connection = await _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<User>(@"SELECT * 
                                                                     FROM Users
                                                                     WHERE Email = @Email", new { Email = emailAddress });
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<User>(@"SELECT * 
                                                                      FROM Users
                                                                      WHERE Id = @Id", new { Id = id });

    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateConnection();
        return await connection.QueryAsync<User>(@"SELECT * 
                                                       FROM Users");
    }

    public async Task<IEnumerable<User>> SearchBySkill(string skill)
    {
        using var connection = await _connectionFactory.CreateConnection();

        return await connection.QueryAsync<User>(@"SELECT *
                                                       FROM Users
                                                       WHERE Skills LIKE '%' || @Skill || '%'", new { Skill = skill });
    }

    public async Task<bool> UpdateAsync(Guid id, User user)
    {
        using var connection = await _connectionFactory.CreateConnection();


        var result = await connection.ExecuteAsync(@"UPDATE Users 
                                                            SET Id = @Id,
                                                            Email = @Email,
                                                            FirstName = @FirstName,
                                                            LastName = @LastName,
                                                            Skills = @Skills,
                                                            DateOfBirth = @DateOfBirth, 
                                                            YearsOfExperience = @YearsOfExperience
                                                            WHERE Id = @IdToUpdate",
            new
            {
                IdToUpdate = id,
                user.Id,
                user.FirstName,
                user.LastName,
                user.Skills,
                user.DateOfBirth,
                user.YearsOfExperience
            });

        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateConnection();

        var result = await connection.ExecuteAsync(@"DELETE FROM Users
                                                            WHERE Id = @Id", new { Id = id });

        return result > 0;
    }
}