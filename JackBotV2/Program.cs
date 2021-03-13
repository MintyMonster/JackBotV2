using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using JackBotV2.Database;
using JackBotV2.Services;
using JackBotV2.Modules;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Tweetinvi;
using Tweetinvi.Parameters;
using Tweetinvi.Exceptions;
using Tweetinvi.Streaming;

namespace JackBotV2
{ 

    class Program
    {

        public static char prefix = '!'; // Prefix needed for command
        private DiscordSocketClient _client; // Set client
        private static string path = @"D:\JackbotLogs\JackBotLog.txt"; // Set data path for log file
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult(); // Start bot

        public async Task MainAsync()
        {

            var token = "ODAwNDg3MzA2NTg1ODMzNTcz.YAS15g.qlWw4A5fL7EZGAjdf7QuXargAKM";

            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                _client = client; // Set services
                

                _client.Log += Log; // Log event handler
                _client.Ready += ReadyAsync; // Bot is ready event handler
                services.GetRequiredService<CommandService>().Log += Log; // log command service connection

                await client.LoginAsync(TokenType.Bot, token); // login to API
                await client.StartAsync(); // Start bot

                await client.SetGameAsync("the Guitar"); // Status message for bot

                await services.GetRequiredService<CommandHandler>().InitialiseAsync(); // initialise command handler

                await Task.Delay(-1);
            }
        }

        private ServiceProvider ConfigureServices() // Set services
        {

            var services = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddDbContext<JackBotEntities>();

            var serviceProvider = services.BuildServiceProvider(); // Build service provider for client
            return serviceProvider; // return the service provider for API
        }

        private Task Log(LogMessage msg) // logging 
        {
            Console.WriteLine(msg.ToString()); // Log the message to console
            using (StreamWriter sw = new StreamWriter(path, true)) // Log the message to file
            {
                sw.WriteLine($"{msg.ToString()}");
            }
            return Task.CompletedTask; // Tell client that task is completed
        }

        private Task ReadyAsync() // Client ready
        {
            Console.WriteLine($"{DateTime.Now} => JackBot online"); // Write to console
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine($"{DateTime.Now} => JackBot **ONLINE**"); // Write to file
            }
            return Task.CompletedTask; // Return task as completed


        }
    }
}
