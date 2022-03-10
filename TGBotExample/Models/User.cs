namespace TGBotExample.Models;

public class User
{
    /// <summary>
    /// ID чата с пользователем
    /// </summary>
    public long id { get; init; }
    /// <summary>
    /// Номер группы
    /// </summary>
    public int group_num { get; init; }
    /// <summary>
    /// Полное имя
    /// </summary>
    public string full_name { get; init; }
    /// <summary>
    /// Конструктор класса User
    /// </summary>
    /// <param name="id">ID чата с пользователем</param>
    /// <param name="group_num">Номер группы</param>
    /// <param name="full_name">Полное имя</param>
    public User(long id, int group_num, string full_name)
    {
        this.id = id;
        this.group_num = group_num;
        this.full_name = full_name;
    }
}