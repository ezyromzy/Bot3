using System;
using Telegram;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


class Program
{
    private static  ITelegramBotClient _botClient;

    private static ReceiverOptions _receiverOptions;

    public static async Task Main()
    {
        _botClient = new TelegramBotClient("Token");
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[]
           {
                UpdateType.Message,
                UpdateType.CallbackQuery
            },

            ThrowPendingUpdates = true,
        };

        using var cts = new CancellationTokenSource();

        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"{me.FirstName} started! ");

        await Task.Delay(-1);
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {

                case UpdateType.Message:
                    {

                        var message = update.Message;

                        var user = message.From;
                        
                        Console.WriteLine($"{user.FirstName} ({user.Id}) wrote a message: {message.Text}");

                        var chat = message.Chat;

                        switch (message.Type)
                        {
                            case MessageType.Text:
                                {
                                    if (message.Text == "/start")
                                    {
                                        await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "Choose keyboard:\n" +
                                            "/inline\n" +
                                            "/reply\n");
                                        return;
                                    }

                                    if (message.Text == "/inline")
                                    {
                                        var inlineKeyboard = new InlineKeyboardMarkup(
                                            new List<InlineKeyboardButton[]>()
                                            {
                                                new InlineKeyboardButton[]
                                                {
                                                    InlineKeyboardButton.WithUrl("This is the button with site", "www.instance.com"),
                                                    InlineKeyboardButton.WithCallbackData("This is just a button", "button1"),

                                                },
                                                new InlineKeyboardButton[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Here is another button", "button2"),
                                                    InlineKeyboardButton.WithCallbackData("And here", "button3")
                                                },
                                            });

                                        await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "This is the inline keyboard!",
                                            replyMarkup: inlineKeyboard);

                                        return;
                                    }

                                    if (message.Text == "/reply")
                                    {
                                        var replyKeyboard = new ReplyKeyboardMarkup(
                                            new List<KeyboardButton[]>()
                                            {
                                                new KeyboardButton[]
                                                {
                                                    new KeyboardButton("Hey!"),
                                                    new KeyboardButton("Bye!"),
                                                },
                                                new KeyboardButton[]
                                                {
                                                    new KeyboardButton("Call me!")
                                                },
                                                new KeyboardButton[]
                                                {
                                                    new KeyboardButton("Write somebody!")
                                                }
                                            })
                                        {
                                            ResizeKeyboard = true,
                                        };

                                        await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "This is the reply keyboard",
                                            replyMarkup: replyKeyboard);

                                        return;
                                    }

                                    if (message.Text == "Call me!")
                                    {
                                        await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "Alright, send me a number!",
                                            replyToMessageId: message.MessageId);

                                        return;
                                    }

                                    if (message.Text == "Write somebody!")
                                    {
                                        await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "Alright, why should i do that?You can do it by yourself",
                                            replyToMessageId: message.MessageId);

                                        return;
                                    }

                                    return;
                                }

                            default:
                                {
                                    await botClient.SendTextMessageAsync(
                                        chat.Id,
                                        "Only use a text!");
                                    return;
                                }
                        }

                        return;
                    }

                case UpdateType.CallbackQuery:
                    {
                        var callbackQuery = update.CallbackQuery;

                        var user = callbackQuery.From;

                        Console.WriteLine($"{user.FirstName} ({user.Id} pressed a button: {callbackQuery.Data})");

                        var chat = callbackQuery.Message.Chat;

                        switch (callbackQuery.Data)
                        {
                            case "button1":
                                {
                                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                    await botClient.SendTextMessageAsync(
                                        chat.Id,
                                        $"You pressed the button {callbackQuery.Data}");
                                    return;
                                }

                            case "button2":
                                {
                                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Random text");

                                    await botClient.SendTextMessageAsync(
                                        chat.Id,
                                        $"You pressed the button {callbackQuery.Data}");
                                    return;
                                }

                            case "button3":
                                {
                                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "This is the full alert text!", showAlert: true);

                                    await botClient.SendTextMessageAsync(
                                        chat.Id,
                                        $"You pressed the button {callbackQuery.Data}");
                                    return;
                                }
                        }

                        return;
                    }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.ToString());
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
           => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
