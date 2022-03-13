using TGBotExample.Services;

namespace TGBotExample.Models;

public interface IDatabaseRepository
{
    public Task CreateClassroom(Classroom classroom);
    public Task CreateGroup(Group group);
    public Task CreateGroupsWeekDays(GroupsWeekDays groupsWeekDays);

    public Task CreateTeacher(Teacher teacher);

    public Task CreateTimeRange(TimeRange timeRange);

    public Task CreateLesson(DBModels dbModels, string groupNum);
    public Task<Classroom> GetClassroom(int id);

    public Task<List<Classroom>> GetClassrooms();

    public Task<Group> GetGroup(int id);

    public Task<List<Group>> GetGroups();

    public Task<GroupsWeekDays> GetGroupsWeekDays(int id);

    public Task<List<GroupsWeekDays>> GetGroupsWeekDayss();

    public Task<Teacher> GetTeacher(int id);

    public Task<List<Teacher>> GetTeachers();

    public Task<TimeRange> GetTimeRange(int id);

    public Task<List<TimeRange>> GetTimeRanges();

    public Task<Lesson> GetLesson(int id);

    public Task<List<Lesson>> GetLessons();
}