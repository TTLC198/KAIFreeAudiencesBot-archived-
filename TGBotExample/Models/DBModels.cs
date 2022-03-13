﻿using Newtonsoft.Json;

namespace TGBotExample.Services;

public class Group
{
    /// <summary>
    /// id
    /// </summary>
    public int id { get; set; }
    /// <summary>
    /// Номер группы
    /// </summary>
    public int group_number { get; set; }
}

public class GroupsWeekDays : Group
{
    /// <summary>
    /// id
    /// </summary>
    public int id { get; set; }
    /// <summary>
    /// День недели
    /// </summary>
    public int week_day { get; set; }
    /// <summary>
    /// Четность недели
    /// </summary>
    public string parity { get; set; }
    /// <summary>
    /// Номер группы
    /// </summary>
    public int group_id { get; set; }
}

public class Classroom
{
    /// <summary>
    /// id
    /// </summary>
    public int id { get; set; }
    /// <summary>
    /// Номер аудитории
    /// </summary>
    public int classroom_number { get; set; }
    /// <summary>
    /// Учебное здание
    /// </summary>
    public int building { get; set; }
}

public class TimeRange
{
    /// <summary>
    /// id
    /// </summary>
    public int id { get; set; }
    /// <summary>
    /// Время начала занятия
    /// </summary>
    public TimeSpan start { get; set; }
    /// <summary>
    /// Время конца занятия
    /// </summary>
    public TimeSpan end { get; set; }
}

public class Teacher
{
    /// <summary>
    /// id
    /// </summary>
    public int id { get; set; }
    /// <summary>
    /// ФИО
    /// </summary>
    public string full_name { get; set; }
}

public class Lesson
{
    /// <summary>
    /// id урока
    /// </summary>
    public int id { get; set; }
    /// <summary>
    /// id расписания
    /// </summary>
    public int schedule_id { get; set; }
    /// <summary>
    /// id временного промежутка
    /// </summary>
    public int time_range_id { get; set; }
    /// <summary>
    /// id аудитории
    /// </summary>
    public int classroom_id { get; set; }
    /// <summary>
    /// id учителя
    /// </summary>
    public int teacher_id { get; set; }
}
/// <summary>
/// Общий класс
/// </summary>
public class DBModels
{
    [JsonProperty("buildNum")] public int building  { get; set; }
    [JsonProperty("dayNum")] public int week_day { get; set; }
    [JsonProperty("audNum")] public int classroom_num { get; set; }
    [JsonProperty("dayTime")] public string start  { get; set; }
    [JsonProperty("prepodName")] public string teacher_name  { get; set; }
    [JsonProperty("dayDate")] public string parity { get; set; }
    [JsonProperty("disciplType")] public string lessonType { get; set; }
}