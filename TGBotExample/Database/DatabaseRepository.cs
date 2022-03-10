using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace TGBotExample.Models;

public class DatabaseRepository : IDatabaseRepository
{
    private string _connectionString;

    public DatabaseRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User> GetUser(long id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            var users = await db.QueryAsync<User>("SELECT * FROM users WHERE id = @id", new {id});
            return users.First();
        }
    }
    
    public async Task<List<User>> GetUsers()
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            var users = await db.QueryAsync<User>("SELECT * FROM users");
            return users.ToList();
        }
    }
    
    public async Task<User> CreateUser(long id, int group_num, string full_name)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            var users = await db.ExecuteAsync("INSERT INTO users VALUES (@id, @froup_num, @full_name)", new {id, group_num, full_name});
            return await GetUser(id);
        }
    }
    
    public async Task<User> CreateUser(User user)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            var users = await db.ExecuteAsync("INSERT INTO users VALUES (@id, @froup_num, @full_name)", new {id = user.id, group_num = user.group_num, full_name = user.full_name});
            return await GetUser(user.id);
        }
    }
    
    public async Task<User> UpdateUser(long id, int group_num, string full_name)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            await db.ExecuteAsync("UPDATE users SET group_num = @group_num, full_name = @full_name WHERE id = @id", new {id = id, group_num = group_num, full_name = full_name});
            return await GetUser(id);
        }
    }
    
    public async Task<User> UpdateUser(User user)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            await db.ExecuteAsync("UPDATE users SET group_num = @group_num, full_name = @full_name WHERE id = @id", new {id = user.id, group_num = user.group_num, full_name = user.full_name});
            return await GetUser(user.id);
        }
    }
    
    public async Task DeleteUser(long id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            await db.ExecuteAsync("DELETE FROM users WHERE id = @id", new {id = id});
        }
    }
    
    public async Task DeleteUser(User user)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            await db.ExecuteAsync("DELETE FROM users WHERE id = @id", new {id = user.id});
        }
    }
}