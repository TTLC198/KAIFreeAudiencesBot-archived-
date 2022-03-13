using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBotExample.Models;
using TGBotExample.Services.TimerJobs;

namespace TGBotExample.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly IServiceProvider _services;

    public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger, IServiceProvider services)
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
            var action = message.Text!.Split(' ')[0] switch
            {
                "sh" => SendSheduleAsync(_botClient, message),
                _ => SendMessageAsync(_botClient, message)
            };
                
            Message sentMessage = await action;
            _logger.LogInformation("The message was sent with id: {sentMessageId}", sentMessage.MessageId);
        }
        catch (ArgumentException e)
        {
            _logger.LogError($"Something went wrong:\n{e.Message}");
            await UnknownMessageHandlerAsync(_botClient, message);
        }
    }

    private async Task<Message> SendSheduleAsync(ITelegramBotClient botClient, Message message)
    {
        var db = _services.CreateScope().ServiceProvider.GetRequiredService<IDatabaseRepository>();

        try
        {
            for (int i = 0; i < 9; i++)
            {
                foreach (var groupId in await Parser.GetGroupsIdAsync(i.ToString()))
                {
                    foreach (var dbmodelss in await Parser.GetScheduleAsync(groupId))
                    {
                        foreach (var dbmodels in dbmodelss)
                        {
                            var groups = await db.GetGroups();
                            await db.CreateLesson(dbmodels, groups.First(gr => gr.id.ToString() == groupId).group_number.ToString());
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Something went wrong!\n" + ex.Message);
        }

        _logger.LogInformation("DB has been updated");
        
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
            text: "Hello!"
        );
    }
    
    private static async Task<Message> UnknownMessageHandlerAsync(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: char.ConvertFromUtf32(0x26A0) + "Произошла ошибка на стороне сервера");
    }   
}