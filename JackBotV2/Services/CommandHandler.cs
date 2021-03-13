using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.IO;

namespace JackBotV2.Services
{
    public class CommandHandler
    {
        private readonly CommandService _commands; // Set services
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private static string path = @"D:\JackbotLogs\JackBotLog.txt";

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>(); // grab services for those pre-set
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync; // Handler for when a command is executed

            _client.MessageReceived += MessageReceivedAsync; // handler for when a message is received
        }

        public async Task InitialiseAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services); // Initialise the client
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            var argPos = 0; // Position to look for the argument
            char prefix = '!'; // prefix needed for commands

            if(!(rawMessage is SocketUserMessage message)) // Check user is not bot
            {
                return;
            }

            if (message.Source != MessageSource.User)  // Check that the message is not from THIS discord bot
            {
                return;
            }

            if(!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix(prefix, ref argPos))) // Make sure that the bot isn't directly mentioned, or has the prefix
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result) // Checks for commands
        {
            var errorEmbed = new EmbedBuilder();
            var errorSb = new StringBuilder();

            if (!command.IsSpecified) // make sure the user specified the command
            {
                System.Console.WriteLine($"{DateTime.Now} => Command failed to execute for {context.User.Username}");

                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    sw.WriteLine($"{DateTime.Now} => Command failed to execute for {context.User.Username}");
                }

                return;
            }

            if (result.IsSuccess) // Make sure that the command was a success
            {
                System.Console.WriteLine($"{DateTime.Now} => {context.User.Username} has executed a command.");

                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    sw.WriteLine($"{DateTime.Now} => {context.User.Username} has executed a command.");
                }

                return;
            }

            Random rnd = new Random();
            int errorNum = rnd.Next(1, 4);

            if(errorNum == 1) // Error messages
            {
                errorSb.AppendLine($"Ket not found");
            } 
            else if(errorNum == 2)
            {
                errorSb.AppendLine($"Not enough ketamine to compute this equation");
            }
            else
            {
                errorSb.AppendLine($"Lacking Egg");
            }

            errorEmbed.Title = "Error 404"; // Setting error embed style
            errorEmbed.Description = errorSb.ToString();
            errorEmbed.Color = new Color(255, 0, 0);

            await context.Channel.SendMessageAsync(null, false, errorEmbed.Build()); // Build error embed
        }
    }
}
