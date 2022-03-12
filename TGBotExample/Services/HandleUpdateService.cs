using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBotExample.Models;

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
        var db = _services.CreateScope().ServiceProvider.GetRequiredService<IDatabaseRepository>();

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
        var parser = await Parser.GetSheduleAsync(message.Text!.Split(' ')[1]);
        
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: parser
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