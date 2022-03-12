using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using TGBotExample.Services;

namespace TGBotExample.Models;

public class DatabaseRepository : IDatabaseRepository
{
    private string _connectionString;

    public DatabaseRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task CreateClassroom(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var classrooms = await db.QueryAsync<Classroom>("SELECT * FROM classroom WHERE id = @id", new {id}); }
    }
    
    public async Task CreateGroups(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var groups = await db.QueryAsync<Group>("SELECT * FROM groups WHERE id = @id", new {id});
        }
    }
    
    public async Task CreateGroupsWeekDays(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var GroupsWeekDayss = await db.QueryAsync<GroupsWeekDays>("SELECT * FROM GroupsWeekDays WHERE id = @id", new {id});
        }
    }
    
    public async Task CreateTeacher(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var teachers = await db.QueryAsync<Teacher>("SELECT * FROM teacher WHERE id = @id", new {id});
        }
    }
    
    public async Task CreateTimeRange(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var timeRanges = await db.QueryAsync<TimeRange>("SELECT * FROM timerange WHERE id = @id", new {id});
        }
    }

    /*public async Task CreateLesson(DBModels dbModels)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            await db.ExecuteAsync("");
                
            var users = await db.ExecuteAsync("INSERT INTO lesson VALUES (@id, @froup_num, @full_name)", new {dbModels.});
            return await GetUser(id);
        }
    }*/
    
    public async Task<Classroom> GetClassroom(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var classrooms = await db.QueryAsync<Classroom>("SELECT * FROM classroom WHERE id = @id", new {id});
            return classrooms.First();
        }
    }
    
    public async Task<Group> GetGroups(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var groups = await db.QueryAsync<Group>("SELECT * FROM groups WHERE id = @id", new {id});
            return groups.First();
        }
    }
    
    public async Task<GroupsWeekDays> GetGroupsWeekDays(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var GroupsWeekDayss = await db.QueryAsync<GroupsWeekDays>("SELECT * FROM GroupsWeekDays WHERE id = @id", new {id});
            return GroupsWeekDayss.First();
        }
    }
    
    public async Task<Teacher> GetTeacher(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var teachers = await db.QueryAsync<Teacher>("SELECT * FROM teacher WHERE id = @id", new {id});
            return teachers.First();
        }
    }
    
    public async Task<TimeRange> GetTimeRange(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var timeRanges = await db.QueryAsync<TimeRange>("SELECT * FROM timerange WHERE id = @id", new {id});
            return timeRanges.First();
        }
    }
    
    public async Task<Lesson> GetLesson(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var lessons = await db.QueryAsync<Lesson>("SELECT * FROM lesson WHERE id = @id", new {id});
            return lessons.First();
        }
    }

    /*public async Task<User> CreateUser(long id, int group_num, string full_name)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            var users = await db.ExecuteAsync("INSERT INTO users VALUES (@id, @froup_num, @full_name)", new {id, group_num, full_name});
            return await GetUser(id);
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
    public async Task DeleteUser(long id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            await db.ExecuteAsync("DELETE FROM users WHERE id = @id", new {id = id});
        }
    }*/
}