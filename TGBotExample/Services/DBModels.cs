using Newtonsoft.Json;

namespace TGBotExample.Services;

public class Groups
{
    public int id { get; set; }
    public int number { get; set; }
}

public class WeekSchedule : Groups
{
    public int id { get; set; }
    public int week_day { get; set; }
    public string parity { get; set; }
    public int group_id { get; set; }
}

public class Classroom
{
    public int id { get; set; }
    public int building { get; set; }
}

public class TimeRange
{
    public int id { get; set; }
    public TimeOnly start { get; set; }
    public TimeOnly end { get; set; }
}

public class Teacher
{
    public int id { get; set; }
    public string name { get; set; }
}

public class Lesson
{
    private WeekSchedule _weekSchedule = new WeekSchedule();
    private TimeRange _timeRange = new TimeRange();
    private Classroom _classroom = new Classroom();
    private Teacher _teacher = new Teacher();
    
    public int schedule_id => _weekSchedule.id;
    public int time_range_id => _timeRange.id;
    public int classroom_id => _classroom.id;
    
    [JsonProperty("disciplNum")] public int id { get; set; }
    [JsonProperty("buildNum")] public int building => _classroom.building;
    [JsonProperty("dayDate")] public string parity => _weekSchedule.parity;
    [JsonProperty("dayTime")] public TimeOnly start => _timeRange.start;
    //public TimeOnly end => ;
    [JsonProperty("prepodName")] public string teacher_name => _teacher.name.ToUpperInvariant();
}