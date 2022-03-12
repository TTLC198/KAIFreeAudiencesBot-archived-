using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TGBotExample.Models;

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
            InlineKeyboardButton.WithCallbackData(text: "Сегодня", callbackData: "0_Automatic"),
            InlineKeyboardButton.WithCallbackData(text: "Ручной ввод", callbackData: "0_byHand"),
        }
    });

    public InlineKeyboardMarkup inlineWeekKeyboard = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Чет", callbackData: "1_even"),
            InlineKeyboardButton.WithCallbackData(text: "Нечет", callbackData: "1_odd"),
        }
    });

    public InlineKeyboardMarkup inlineDayKeyboard = new(new[]
    {
        new[] {InlineKeyboardButton.WithCallbackData(text: "Пн", callbackData: "2_Mon")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "Вт", callbackData: "2_Tue")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "Ср", callbackData: "2_Wed")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "Чт", callbackData: "2_Thu")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "Пт", callbackData: "2_Fri")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "Сб", callbackData: "2_Sat")},
    });

    public InlineKeyboardMarkup inlineTimeKeyboard = new(new[]
    {
        new[] {InlineKeyboardButton.WithCallbackData(text: "8:00 - 9:30", callbackData: "3_First")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "9:40 - 11:10", callbackData: "3_Second")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "11:20 - 12:50", callbackData: "3_Third")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "13:30 - 15:00", callbackData: "3_Fourth")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "15:10 - 16:40", callbackData: "3_Fifth")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "16:50 - 18:20", callbackData: "3_Sixth")},
        new[] {InlineKeyboardButton.WithCallbackData(text: "18:30 - 20:00", callbackData: "3_Seventh")}
    });

    public InlineKeyboardMarkup inlineYNBuildingKeyboard = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Yes", callbackData: "4_Yes"),
            InlineKeyboardButton.WithCallbackData(text: "No", callbackData: "4_No")
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
            InlineKeyboardButton.WithCallbackData(text: "Yes", callbackData: "6_Yes"),
            InlineKeyboardButton.WithCallbackData(text: "No", callbackData: "6_No")
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
        var db = _services.CreateScope().ServiceProvider.GetRequiredService<IDatabaseRepository>();

        try
        {
            Task<Message>? action = null;
            if (_resultStrings[6] == "Yes")
            {
                action = TypeRoom(_botClient, message);
            }
            else
            {
                action = message.Text!.Split(' ')[0] switch
                {
                    "/start" => OnStart(_botClient, message),
                    _ => SendMessageAsync(_botClient, message)
                };
            }

            Message sentMessage = await action;
            _logger.LogInformation("The message was sent with id: {sentMessageId}", sentMessage.MessageId);
        }
        catch (ArgumentException e)
        {
            _logger.LogError($"Something went wrong:\n{e.Message}");
            await UnknownMessageHandlerAsync(_botClient, message);
        }
    }

    private async Task<Message> SendMessageAsync(ITelegramBotClient botClient, Message message)
    {
        var db = _services.CreateScope().ServiceProvider.GetRequiredService<IDatabaseRepository>();

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Hello!"
        );
    }

    private async Task<Message> OnStart(ITelegramBotClient botClient, Message message)
    {
        _resultStrings = new string[8];


        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            replyMarkup: inlineModeKeyboard,
            text: "Chose mode"
        );
    }

    private async Task<Message> TypeRoom(ITelegramBotClient botClient, Message message)
    {
        

        _resultStrings[7] = message.Text!;
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text:
            $"You chose mode: byHand\nYou chose {_resultStrings[1]} type of week\n You chose {_resultStrings[2]} day\n You chose {_resultStrings[3]}\n You chose {_resultStrings[5]}\n You typed {_resultStrings[7]}",
            replyMarkup: inlineAllRightKeyboard);
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
                    if (_resultStrings[0] == "byHand")
                    {
                        action = await botClient.EditMessageTextAsync(
                            chatId: query.Message!.Chat.Id,
                            messageId: query.Message.MessageId,
                            text: $"You chose mode: {_resultStrings[0]}\nNow choose type of week",
                            replyMarkup: inlineWeekKeyboard);
                    }
                    else
                    {
                        DateTime myDateTime = DateTime.Now;
                        int firstDayOfYear = (int) new DateTime(myDateTime.Year, 1, 1).DayOfWeek;
                        _resultStrings[1] = ((myDateTime.DayOfYear + firstDayOfYear) / 7 + 1) % 2 == 1 ? "odd" : "even";
                        _resultStrings[2] = myDateTime.DayOfWeek.ToString();
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
                            "First", "",
                            "Second", "",
                            "Third", "",
                            "Fourth", "",
                            "Fifth", "",
                            "Sixth", "",
                            "Seventh"
                        };

                        for (int i = 0; i < timesOfLessons.Length; i += 2)
                        {
                            if (myDateTime >= timesOfLessons[i] && myDateTime < timesOfLessons[i + 1])
                            {
                                _resultStrings[3] = timeStrings[i];
                            }
                        }

                        action = await botClient.EditMessageTextAsync(
                            chatId: query.Message!.Chat.Id,
                            messageId: query.Message.MessageId,
                            text:
                            $"You chose mode: {_resultStrings[0]}\nYou chose {_resultStrings[1]} type of week\n You chose {_resultStrings[2]} day\n You chose {_resultStrings[3]}\n Будешь выбирать здание?",
                            replyMarkup: inlineYNBuildingKeyboard);
                    }

                    break;
                case '1':
                    _resultStrings[1] = query.Data.ToString()[2..];
                    action = await botClient.EditMessageTextAsync(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        $"You chose mode: {_resultStrings[0]}\nYou chose {_resultStrings[1]} type of week\n Now choose day of week",
                        replyMarkup: inlineDayKeyboard);
                    break;
                case '2':
                    _resultStrings[2] = query.Data.ToString()[2..];
                    action = await botClient.EditMessageTextAsync(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        $"You chose mode: {_resultStrings[0]}\nYou chose {_resultStrings[1]} type of week\n You chose {_resultStrings[2]} day\n Now chose time",
                        replyMarkup: inlineTimeKeyboard);
                    break;
                case '3':
                    _resultStrings[3] = query.Data.ToString()[2..];
                    action = await botClient.EditMessageTextAsync(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        $"You chose mode: {_resultStrings[0]}\nYou chose {_resultStrings[1]} type of week\n You chose {_resultStrings[2]} day\n You chose {_resultStrings[3]}\n Будешь выбирать здание?",
                        replyMarkup: inlineYNBuildingKeyboard);
                    break;
                case '4':
                    _resultStrings[4] = query.Data.ToString()[2..];
                    if (_resultStrings[4] == "Yes")
                    {
                        action = await botClient.EditMessageTextAsync(
                            chatId: query.Message!.Chat.Id,
                            messageId: query.Message.MessageId,
                            text:
                            $"You chose mode: {_resultStrings[0]}\nYou chose {_resultStrings[1]} type of week\n You chose {_resultStrings[2]} day\n You chose {_resultStrings[3]}",
                            replyMarkup: inlineBuildingKeyboard);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    break;
                case '5':
                    _resultStrings[5] = query.Data.ToString()[2..];
                    action = await botClient.EditMessageTextAsync(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        $"You chose mode: {_resultStrings[0]}\nYou chose {_resultStrings[1]} type of week\n You chose {_resultStrings[2]} day\n You chose {_resultStrings[3]}\n You chose {_resultStrings[5]}\n Будешь выбирать кабинет?",
                        replyMarkup: inlineYNRoomKeyboard);
                    break;
                case '6':
                    _resultStrings[6] = query.Data.ToString()[2..];
                    if (_resultStrings[6] == "Yes")
                    {
                        action = await botClient.EditMessageTextAsync(
                            chatId: query.Message!.Chat.Id,
                            messageId: query.Message.MessageId,
                            text:
                            $"You chose mode: {_resultStrings[0]}\nYou chose {_resultStrings[1]} type of week\n You chose {_resultStrings[2]} day\n You chose {_resultStrings[3]}\n You chose {_resultStrings[5]}\n Напишите кабинет");
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    break;
                case '7':
                    if (query.Data.ToString()[2..] == "Yes")
                    {
                        var str = _resultStrings.Aggregate("",
                            (current, resultString) => current + (resultString + " "));
                        _logger.LogInformation($"Request: {str}");
                        throw new NotImplementedException();
                    }
                    else
                    {
                        _resultStrings = new string[8];
                        action = await botClient.SendTextMessageAsync(
                            chatId: query.Message!.Chat.Id,
                            replyMarkup: inlineModeKeyboard,
                            text: "Choose mode:");
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

    private static async Task<Message> UnknownMessageHandlerAsync(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: char.ConvertFromUtf32(0x26A0) + "Произошла ошибка на стороне сервера");
    }
}