using System.Data;
using System.Security;
using Dapper;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TGBotExample.Models;
using TGBotExample.Services.TimerJobs;


namespace TGBotExample.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly IServiceProvider _services;

    public static string[] _resultStrings = new string[8];


    public InlineKeyboardMarkup inlineModeKeyboard = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Автоматический ввод", callbackData: "0_Автоматический ввод"),
            InlineKeyboardButton.WithCallbackData(text: "Ручной ввод", callbackData: "0_Ручной ввод"),
        }
    });

    public InlineKeyboardMarkup inlineWeekKeyboard = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Чет", callbackData: "1_Четная"),
            InlineKeyboardButton.WithCallbackData(text: "Нечет", callbackData: "1_Нечетная"),
        }
    });

    public InlineKeyboardMarkup inlineDayKeyboard = new(new[]
    {
        new[] {InlineKeyboardButton.WithCallbackData(text: "Пн", callbackData: "2_Понедельник")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "Вт", callbackData: "2_Вторник")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "Ср", callbackData: "2_Среда")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "Чт", callbackData: "2_Четверг")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "Пт", callbackData: "2_Пятница")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "Сб", callbackData: "2_Суббота")},
    });

    public InlineKeyboardMarkup inlineTimeKeyboard = new(new[]
    {
        new[] {InlineKeyboardButton.WithCallbackData(text: "8:00 - 9:30", callbackData: "3_8:00 - 9:30")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "9:40 - 11:10", callbackData: "3_9:40 - 11:10")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "11:20 - 12:50", callbackData: "3_11:20 - 12:50")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "13:30 - 15:00", callbackData: "3_13:30 - 15:00")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "15:10 - 16:40", callbackData: "3_15:10 - 16:40")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "16:50 - 18:20", callbackData: "3_16:50 - 18:20")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "18:30 - 20:00", callbackData: "3_18:30 - 20:00")}
    });

    public InlineKeyboardMarkup inlineYNBuildingKeyboard = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Да", callbackData: "4_Yes"),
            InlineKeyboardButton.WithCallbackData(text: "Нет", callbackData: "4_No")
        }
    });

    public InlineKeyboardMarkup inlineBuildingKeyboard = new(new[]
    {
        new[] {InlineKeyboardButton.WithCallbackData(text: "1", callbackData: "5_1")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "2", callbackData: "5_2")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "3", callbackData: "5_3")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "4", callbackData: "5_4")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "5", callbackData: "5_5")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "6", callbackData: "5_6")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "7", callbackData: "5_7")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "8", callbackData: "5_8")},
    });

    public InlineKeyboardMarkup inlineYNRoomKeyboard = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Да", callbackData: "6_Yes"),
            InlineKeyboardButton.WithCallbackData(text: "Нет", callbackData: "6_No")
        }
    });

    public InlineKeyboardMarkup inlineAllRightKeyboard = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Все хорошо", callbackData: "7_Yes"),
            InlineKeyboardButton.WithCallbackData(text: "Изменить", callbackData: "7_No")
        }
    });

    public ReplyKeyboardMarkup firstChoice = new(new[]
    {
        new KeyboardButton[] {"Узнать свободные аудитории", "Расписание"}
    })
    {
        ResizeKeyboard = true
    };
    public ReplyKeyboardMarkup Back = new(new[]
    {
        new KeyboardButton[] {"Назад"}
    })
    {
        ResizeKeyboard = true
    };

    public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger,
        IServiceProvider services)
    {
        _botClient = botClient;
        _logger = logger;
        _services = services;
    }


    private Task HandleErrorAsync(Exception exception)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }

    public async Task EchoAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(update.Message!),
            UpdateType.CallbackQuery => QueryUpdate(_botClient, update.CallbackQuery!),
            _ => UnknownMessageHandlerAsync(_botClient, update.Message!)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    private async Task BotOnMessageReceived(Message message)
    {
        _logger.LogInformation("Receive message type: {messageType}", message.Type);

        try
        {
            Task<Message>? action = null;
            if (_resultStrings[6] == "Yes" && int.TryParse(message.Text, out var res))
            {
                action = TypeRoom(_botClient, message);
                _resultStrings[6] = "No";
            }
            else
            {
                action = message.Text!.Split(' ')[0] switch
                {
                    "/start" => OnStart(_botClient, message),
                    "Узнать" => FreeRoom(_botClient, message),
                    "Назад" => OnStart(_botClient, message),
                    "sh" => SendSheduleAsync(_botClient, message),
                    _ => SendMessageAsync(_botClient, message)
                };
            }

            Message sentMessage = await action;
            _logger.LogInformation("The message was sent with id: {sentMessageId}", sentMessage.MessageId);
        }
        catch (Exception e)
        {
            _logger.LogError($"Something went wrong:\n{e.Message}");
            switch (e.Message)
            {
                case "Sunday":
                    Message error1Message = await _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "В воскресенье все аудитории свободны)");
                    break;
                case "OutOfTime":
                    Message error2Message = await _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Вы пытаетесь посмотреть аудитории слишком поздно, все они уже свободны)");
                    break;
                default:
                    await UnknownMessageHandlerAsync(_botClient, message);
                    break;
            }
        }
    }

    private async Task<Message> SendSheduleAsync(ITelegramBotClient botClient, Message message)
    {
        //var db = _services.CreateScope().ServiceProvider.GetRequiredService<IDatabaseRepository>();

        //try
        //{
        //    for (int i = 0; i < 9; i++)
        //    {
        //        foreach (var groupId in await Parser.GetGroupsIdAsync(i.ToString()))
        //        {
        //            foreach (var dbmodelss in await Parser.GetScheduleAsync(groupId))
        //            {
        //                foreach (var dbmodels in dbmodelss)
        //                {
        //                    var groups = await db.GetGroups();
        //                    await db.CreateLesson(dbmodels,
        //                        groups.First(gr => gr.id.ToString() == groupId).group_number.ToString());
        //                }
        //            }
        //        }
        //    }
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogCritical("Something went wrong!\n" + ex.Message);
        //}

        //_logger.LogInformation("DB has been updated");

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Update db has been successfully!"
        );
    }

    private async Task<Message> SendMessageAsync(ITelegramBotClient botClient, Message message)
    {
        var db = _services.CreateScope().ServiceProvider.GetRequiredService<IDatabaseRepository>();

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Я не знаю такую комманду"
        );
    }

    private async Task<Message> OnStart(ITelegramBotClient botClient, Message message)
    {
        _resultStrings = new string[8];
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            replyMarkup: firstChoice,
            text:
            "Привет пользователь! Я бот помошник, помогу найти тебе свободную аудиторию! Выбери дольнейшее действие!",
            cancellationToken: CancellationToken.None
        );
    }

    private async Task<Message> FreeRoom(ITelegramBotClient botClient, Message message)
    {
        _resultStrings = new string[8];
        Message mes = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Отлично, приступим!",
            replyMarkup: new ReplyKeyboardRemove());
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            replyMarkup: inlineModeKeyboard,
            text:
            "Выбери режим действий",
            cancellationToken: CancellationToken.None
        );
    }

    private async Task<Message> TypeRoom(ITelegramBotClient botClient, Message message)
    {
        _resultStrings[7] = message.Text!;
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text:
            $"Ты выбрал режим: {_resultStrings[0]}\nТы выбрал четность недели: {_resultStrings[1]}\nТы выбрал день недели: {_resultStrings[2]}\nТы выбрал временной промежуток: {_resultStrings[3]}\n Ты выбрал корпус: {_resultStrings[5]}\n Ты выбрал комнату {_resultStrings[7]}",
            replyMarkup: inlineAllRightKeyboard,
            cancellationToken: CancellationToken.None);
    }

    private async Task QueryUpdate(ITelegramBotClient botClient, CallbackQuery query)
    {
        try
        {
            Message action;
            switch (query.Data[0])
            {
                case '0':
                    _resultStrings[0] = query.Data.ToString()[2..];
                    if (_resultStrings[0] == "Ручной ввод")
                    {
                        action = await botClient.EditMessageTextAsync(
                            chatId: query.Message!.Chat.Id,
                            messageId: query.Message.MessageId,
                            text: $"Ты выбрал режим: {_resultStrings[0]}\nВыбери четность недели",
                            replyMarkup: inlineWeekKeyboard,
                            cancellationToken: CancellationToken.None);
                    }
                    else
                    {
                        DateTime myDateTime = DateTime.Now;
                        int firstDayOfYear = (int) new DateTime(myDateTime.Year, 1, 1).DayOfWeek;
                        _resultStrings[1] = ((myDateTime.DayOfYear + firstDayOfYear) / 7 + 1) % 2 == 1
                            ? "Нечетная"
                            : "Четная";
                        _resultStrings[2] = myDateTime.DayOfWeek.ToString() switch
                        {
                            "Monday" => "Понедельник",
                            "Tuesday" => "Вторник",
                            "Wednesday" => "Среда",
                            "Thursday" => "Четверг",
                            "Friday" => "Пятница",
                            "Saturday" => "Суббота",
                            _ => throw new Exception("Sunday")
                        };
                        DateTime[] timesOfLessons = new DateTime[]
                        {
                            DateTime.Today.Add(new TimeSpan(8, 0, 0)),
                            DateTime.Today.Add(new TimeSpan(9, 30, 0)),
                            DateTime.Today.Add(new TimeSpan(9, 40, 0)),
                            DateTime.Today.Add(new TimeSpan(11, 10, 0)),
                            DateTime.Today.Add(new TimeSpan(11, 20, 0)),
                            DateTime.Today.Add(new TimeSpan(12, 50, 0)),
                            DateTime.Today.Add(new TimeSpan(13, 30, 0)),
                            DateTime.Today.Add(new TimeSpan(15, 0, 0)),
                            DateTime.Today.Add(new TimeSpan(15, 10, 0)),
                            DateTime.Today.Add(new TimeSpan(16, 40, 0)),
                            DateTime.Today.Add(new TimeSpan(16, 50, 0)),
                            DateTime.Today.Add(new TimeSpan(18, 20, 0)),
                            DateTime.Today.Add(new TimeSpan(18, 30, 0)),
                            DateTime.Today.Add(new TimeSpan(20, 0, 0)),
                        };

                        string[] timeStrings =
                        {
                            "8:00 - 9:30", "",
                            "9:40 - 11:10", "",
                            "11:20 - 12:50", "",
                            "13:30 - 15:00", "",
                            "15:10 - 16:40", "",
                            "16:50 - 18:20", "",
                            "18:30 - 20:00"
                        };

                        for (int i = 0; i < timesOfLessons.Length; i += 2)
                        {
                            if (myDateTime < timesOfLessons[i] || myDateTime >= timesOfLessons[i + 1]) continue;
                            _resultStrings[3] = timeStrings[i];
                            break;
                        }

                        if (_resultStrings[3] == "") throw new Exception("OutOfTime");

                        action = await botClient.EditMessageTextAsync(
                            chatId: query.Message!.Chat.Id,
                            messageId: query.Message.MessageId,
                            text:
                            $"Ты выбрал режим: {_resultStrings[0]}\nТы выбрал четность недели: {_resultStrings[1]}\nТы выбрал день недели: {_resultStrings[2]}\nТы выбрал временной промежуток: {_resultStrings[3]}\nБудешь выбирать здание?",
                            replyMarkup: inlineYNBuildingKeyboard,
                            cancellationToken: CancellationToken.None);
                    }

                    break;
                case '1':
                    _resultStrings[1] = query.Data.ToString()[2..];
                    action = await botClient.EditMessageTextAsync(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        $"Ты выбрал режим: {_resultStrings[0]}\nТы выбрал четность недели: {_resultStrings[1]}\nТеперь выбери день недели",
                        replyMarkup: inlineDayKeyboard,
                        cancellationToken: CancellationToken.None);
                    break;
                case '2':
                    _resultStrings[2] = query.Data.ToString()[2..];
                    action = await botClient.EditMessageTextAsync(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        $"Ты выбрал режим: {_resultStrings[0]}\nТы выбрал четность недели:{_resultStrings[1]}\nТы выбрал день недели:  {_resultStrings[2]}\nМожешь выбрать временной промежуток",
                        replyMarkup: inlineTimeKeyboard,
                        cancellationToken: CancellationToken.None);
                    break;
                case '3':
                    _resultStrings[3] = query.Data.ToString()[2..];
                    action = await botClient.EditMessageTextAsync(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        $"Ты выбрал режим: {_resultStrings[0]}\nТы выбрал четность недели:{_resultStrings[1]}\nТы выбрал день недели: {_resultStrings[2]}\nТы выбрал временной промежуток: {_resultStrings[3]}\nБудешь выбирать здание?",
                        replyMarkup: inlineYNBuildingKeyboard,
                        cancellationToken: CancellationToken.None);
                    break;
                case '4':
                    _resultStrings[4] = query.Data.ToString()[2..];
                    if (_resultStrings[4] == "Yes")
                    {
                        action = await botClient.EditMessageTextAsync(
                            chatId: query.Message!.Chat.Id,
                            messageId: query.Message.MessageId,
                            text:
                            $"Ты выбрал режим: {_resultStrings[0]}\nТы выбрал четность недели:{_resultStrings[1]}\nТы выбрал день недели: {_resultStrings[2]}\nТы выбрал временной промежуток: {_resultStrings[3]}",
                            replyMarkup: inlineBuildingKeyboard,
                            cancellationToken: CancellationToken.None);
                    }
                    else
                    {
                        var str = _resultStrings.Aggregate("",
                            (current, resultString) => current + (resultString + " "));
                        _logger.LogInformation($"Request: {str}");
                        await ThreePar(_resultStrings, query.Message!);
                    }

                    break;
                case '5':
                    _resultStrings[5] = query.Data.ToString()[2..];
                    action = await botClient.EditMessageTextAsync(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        $"Ты выбрал режим: {_resultStrings[0]}\nТы выбрал четность недели:{_resultStrings[1]}\nТы выбрал день недели: {_resultStrings[2]}\nТы выбрал временной промежуток: {_resultStrings[3]}\nТы выбрал корпус: {_resultStrings[5]}\nБудешь выбирать кабинет?",
                        replyMarkup: inlineYNRoomKeyboard,
                        cancellationToken: CancellationToken.None);
                    break;
                case '6':
                    _resultStrings[6] = query.Data.ToString()[2..];
                    if (_resultStrings[6] == "Yes")
                    {
                        action = await botClient.EditMessageTextAsync(
                            chatId: query.Message!.Chat.Id,
                            messageId: query.Message.MessageId,
                            text:
                            $"Ты выбрал режим: {_resultStrings[0]}\nТы выбрал четность недели:{_resultStrings[1]}\nТы выбрал день недели: {_resultStrings[2]}\nТы выбрал временной промежуток: {_resultStrings[3]}\nТы выбрал корпус: {_resultStrings[5]}\n Напишите кабинет",
                            cancellationToken: CancellationToken.None);
                    }
                    else
                    {
                        var str = _resultStrings.Aggregate("",
                            (current, resultString) => current + (resultString + " "));
                        _logger.LogInformation($"Request: {str}");
                        await FourPar(_resultStrings, query.Message!);
                    }

                    break;
                case '7':
                    if (query.Data.ToString()[2..] == "Yes")
                    {
                        _resultStrings = new string[8];
                        var str = _resultStrings.Aggregate("",
                            (current, resultString) => current + (resultString + " "));
                        _logger.LogInformation($"Request: {str}");
                        await FifPar(_resultStrings, query.Message!);
                    }
                    else
                    {
                        action = await botClient.SendTextMessageAsync(
                            chatId: query.Message!.Chat.Id,
                            replyMarkup: inlineModeKeyboard,
                            text: "Выбери режим работы",
                            cancellationToken: CancellationToken.None);
                    }

                    break;
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Something went wrong:\n{e.Message}");
            await UnknownMessageHandlerAsync(_botClient, query.Message);
        }
    }

    private string[] Translate(string[] humanStrings)
    {
        humanStrings[1] = humanStrings[1] switch
        {
            "Нечётная" => "0",
            "Четная" => "1",
            _ => ""
        };
        humanStrings[2] = humanStrings[2] switch
        {
            "Понедельник" => "1",
            "Вторник" => "2",
            "Среда" => "3",
            "Четверг" => "4",
            "Пятница" => "5",
            "Суббота" => "6",
            _ => ""
        };
        humanStrings[3] = humanStrings[3] switch
        {
            "8:00 - 9:30" => "1",
            "9:40 - 11:10" => "2",
            "11:20 - 12:50" => "3",
            "13:30 - 15:00" => "4",
            "15:10 - 16:40" => "5",
            "16:50 - 18:20" => "6",
            "18:30 - 20:00" => "7",
            _ => ""
        };

        return humanStrings;
    }

    private async Task ThreePar(string[] threeStrings, Message message)
    {
        threeStrings = Translate(threeStrings);
        List<Classroom> exampleClassRoom = new();
        List<Lesson> exampleLessons = new();
        List<GroupsWeekDays> exampleDaysList = new();
        var groupDays = exampleDaysList
            .Where(wd => wd.parity == threeStrings[1] && wd.week_day == int.Parse(threeStrings[2])).ToList();
        var lessons = exampleLessons
            .Where(le => le.time_range_id == int.Parse(threeStrings[3]) && le.schedule_id == groupDays[0].id).ToList();
        var classRoom = new List<Classroom>();
        foreach (var lesson in lessons)
        {
            classRoom.AddRange(exampleClassRoom.Where(cl => cl.id == lesson.classroom_id));
        }

        for (int i = 0; i < 8; i++)
        {
            Message sentMessage = await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text:
                $"Корпус {i + 1}\n В этот период времени заняты: {classRoom.Where(cl => cl.building == i).Aggregate("", (current, row) => current + (row + ", "))}",
                replyMarkup: Back);
        }
    }

    private async Task FourPar(string[] fourStrings, Message message)
    {
        fourStrings = Translate(fourStrings);
        List<Classroom> exampleClassRoom = new();
        List<Lesson> exampleLessons = new();
        List<GroupsWeekDays> exampleDaysList = new();
        var groupDays = exampleDaysList
            .Where(wd => wd.parity == fourStrings[1] && wd.week_day == int.Parse(fourStrings[2])).ToList();
        var lessons = exampleLessons
            .Where(le => le.time_range_id == int.Parse(fourStrings[3]) && le.schedule_id == groupDays[0].id).ToList();
        var classRoom = new List<Classroom>();
        foreach (var lesson in lessons)
        {
            classRoom.AddRange(exampleClassRoom.Where(cl => cl.id == lesson.classroom_id));
        }
        Message sentMessage = await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text:
            $"Корпус {int.Parse(fourStrings[5]) + 1}\n В этот период времени заняты: {classRoom.Where(cl => cl.building == int.Parse(fourStrings[5])).Aggregate("", (current, row) => current + (row + ", "))}",
            replyMarkup: Back);
    }

    private async Task FifPar(string[] fifeStrings, Message message)
    {
        fifeStrings = Translate(fifeStrings);
        List<Classroom> exampleClassRoom = new();
        List<Lesson> exampleLessons = new();
        List<GroupsWeekDays> exampleDaysList = new();
        List<Teacher> exampleTeachers = new();
        if (exampleClassRoom.Count(cl =>
                cl.building == int.Parse(fifeStrings[5]) && cl.classroom_number == int.Parse(fifeStrings[7])) == 0)
        {
            var classRooms = exampleClassRoom.Where(cl =>
                cl.building == int.Parse(fifeStrings[5]) && cl.classroom_number == int.Parse(fifeStrings[7])).ToList();
            var groupDays = exampleDaysList
                .Where(wd => wd.parity == fifeStrings[1] && wd.week_day == int.Parse(fifeStrings[2])).ToList();
            var lessons = exampleLessons.Where(le =>
                le.time_range_id == int.Parse(fifeStrings[3]) && le.classroom_id == classRooms[0].id &&
                le.schedule_id == groupDays[0].id).ToList();
            if (lessons.Any())
            {
                foreach (var lesson in lessons)
                {
                    Message lessonMes = await _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text:
                        $"Room {classRooms[0].classroom_number}, Teacher: {exampleTeachers.Where(tc => tc.id == lesson.teacher_id).ToString()}, group: {groupDays[0].group_id}",
                        replyMarkup: Back);
                }
            }
            else
            {
                Message lessonMes = await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "В данный момент аудитория свободна",
                    replyMarkup: Back);
            }
        }
        else
        {
            throw new Exception("NotRoomExists");
        }
    }

    private static async Task<Message> UnknownMessageHandlerAsync(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: char.ConvertFromUtf32(0x26A0) + "Произошла ошибка на стороне сервера");
    }
}