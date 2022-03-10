namespace TGBotExample.Models;

public interface IDatabaseRepository
{
    /// <summary>
    /// Получение пользователя
    /// </summary>
    /// <param name="id">id</param>
    /// <returns>Экземпляр пользователя</returns>
    public Task<User> GetUser(long id);
    /// <summary>
    /// олучение коллекции пользователей
    /// </summary>
    /// <returns>Коллекция пользователей</returns>
    public Task<List<User>> GetUsers();
    /// <summary>
    /// Создание нового пользователя
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="group_num">Номер группы</param>
    /// <param name="full_name">Имя</param>
    /// <returns>Экземпляр пользователя</returns>
    public Task<User> CreateUser(long id, int group_num, string full_name);
    /// <summary>
    /// Создание нового пользователя
    /// </summary>
    /// <param name="user">Экземпляр пользователя</param>
    /// <returns>Экземпляр пользователя</returns>
    public Task<User> CreateUser(User user);
    /// <summary>
    /// Редактирование пользователя
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="group_num">Номер группы</param>
    /// <param name="full_name">Имя</param>
    /// <returns>Экземпляр пользователя</returns>
    public Task<User> UpdateUser(long id, int group_num, string full_name);
    /// <summary>
    /// Редактирование пользователя
    /// </summary>
    /// <param name="user">Экземпляр пользователя</param>
    /// <returns>Экземпляр пользователя</returns>
    public Task<User> UpdateUser(User user);
    /// <summary>
    /// Удаление пользователя по id
    /// </summary>
    /// <param name="id">id</param>
    public Task DeleteUser(long id);
    /// <summary>
    /// Удаление пользователя по экземпляру пользователя
    /// </summary>
    /// <param name="user">Экземпляр пользователя</param>
    /// <returns>Экземпляр пользователя</returns>
    public Task DeleteUser(User user);
}