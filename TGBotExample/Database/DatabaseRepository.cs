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
    
    public async Task CreateClassroom(Classroom classroom)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            await db.ExecuteAsync("INSERT INTO classroom VALUES (@classroom_number, @building)", new { classroom.classroom_number, classroom.building}); 
        }
    }
    
    public async Task CreateGroup(Group group)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            await db.ExecuteAsync("INSERT INTO groups VALUES (@id, @group_id_ref)", new {id = group.id, group_id_ref = group.group_number});
        }
    }
    
    public async Task CreateGroupsWeekDays(GroupsWeekDays groupsWeekDays)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            await db.ExecuteAsync("INSERT INTO groupsweekdays VALUES (@week_day, @parity, @group_id)", new {groupsWeekDays.week_day, groupsWeekDays.parity, groupsWeekDays.group_id});
        }
    }
    
    public async Task CreateTeacher(Teacher teacher)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            await db.ExecuteAsync("INSERT INTO teacher VALUES (@full_name)", new {teacher.full_name});
        }
    }
    
    public async Task CreateTimeRange(TimeRange timeRange)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            await db.ExecuteAsync("INSERT INTO timerange VALUES (@start_time, @end_time)", new {timeRange.start, timeRange.end});
        }
    }

    public async Task CreateLesson(DBModels dbModels, string groupNum)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            await CreateGroup(new Group() { group_number = int.Parse(groupNum)});
            
            var groups = await GetGroups();
            await CreateClassroom(new Classroom()
                { building = dbModels.building, classroom_number = dbModels.classroom_num });
            var teacherName = dbModels.teacher_name.ToLowerInvariant().ToList();
            teacherName.ForEach(fn => fn = teacherName.IndexOf(fn) == 0 ? fn.ToString().ToUpper()[0] : fn);
            
            await CreateTeacher(new Teacher() { full_name = teacherName.ToArray().ToString()!});
            await CreateGroupsWeekDays(new GroupsWeekDays()
            {
                group_number = int.Parse(groupNum), week_day = dbModels.week_day, parity = dbModels.parity,
                group_id = groups.First(g => g.group_number.ToString() == groupNum).id
            });
            await db.ExecuteAsync("INSERT INTO lesson VALUES (@shedule_id, @time_range_id, @classroom_id, @teacher_id)", 
                new {  });
        }
    }
    
    public async Task<Classroom> GetClassroom(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var classrooms = await db.QueryAsync<Classroom>("SELECT * FROM classroom WHERE id = @id", new {id});
            return classrooms.First();
        }
    }
    
    public async Task<List<Classroom>> GetClassrooms()
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var classrooms = await db.QueryAsync<Classroom>("SELECT * FROM classroom");
            return classrooms.ToList();
        }
    }
    
    public async Task<Group> GetGroup(int id)
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var groups = await db.QueryAsync<Group>("SELECT * FROM groups WHERE id = @id", new {id});
            return groups.First();
        }
    }
    
    public async Task<List<Group>> GetGroups()
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var groups = await db.QueryAsync<Group>("SELECT * FROM groups");
            return groups.ToList();
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
    
    public async Task<List<GroupsWeekDays>> GetGroupsWeekDayss()
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var GroupsWeekDayss = await db.QueryAsync<GroupsWeekDays>("SELECT * FROM GroupsWeekDays");
            return GroupsWeekDayss.ToList();
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
    
    public async Task<List<Teacher>> GetTeachers()
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var teachers = await db.QueryAsync<Teacher>("SELECT * FROM teacher");
            return teachers.ToList();
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
    
    public async Task<List<TimeRange>> GetTimeRanges()
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var timeRanges = await db.QueryAsync<TimeRange>("SELECT * FROM timerange");
            return timeRanges.ToList();
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
    
    public async Task<List<Lesson>> GetLessons()
    {
        using (IDbConnection db = new SqlConnection(_connectionString))
        { 
            var lessons = await db.QueryAsync<Lesson>("SELECT * FROM lesson");
            return lessons.ToList();
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