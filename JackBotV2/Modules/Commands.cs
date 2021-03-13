using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using JackBotV2.Database;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Net.Http;

namespace JackBotV2.Modules
{
    public class Commands : ModuleBase
    {
        private readonly JackBotEntities _db; // set _db for later use in IServiceProvider
        private static string path = @"C:\Users\camer\source\repos\JackBotV2\JackBotV2\JackBotLog.txt"; // Specify path to log file

        public Commands(IServiceProvider services) // Give _db the service it requires to hook into JackBotEntities 
        {
            _db = services.GetRequiredService<JackBotEntities>();
        }

        public static void LogToFile(string str, string args = null) // Logging commands that are done via a .txt format 
        {
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {str}: {args}");
            }
        }

        [Command("add")] // add command
        [Alias("a")] // give alias to command !a !add
        [RequireUserPermission(GuildPermission.MuteMembers)] // require permission (muteMembers is generally a higher level command not for everyone)
        public async Task AddQuote([Remainder]string quote = null)  // Starts the add quote
        {
            var sb = new StringBuilder(); 
            var embed = new EmbedBuilder();
            //var errorSb = new StringBuilder();
            //var errorEmbed = new EmbedBuilder();

            var user = Context.User; // Grab the user
            var userForDb = Context.User.ToString(); // DB can only have strings inputted
            
            if(string.IsNullOrEmpty(quote)) // Error checking
            {
                sb.AppendLine($"You need to supply a quote!\n!add [insert quote here]");
            }

            await _db.AddAsync(new JackBotQuotes // Assign Db's values
            { 
                Quote = quote,
                User = userForDb
            });

            await _db.SaveChangesAsync(); // save Database
            sb.AppendLine($"{user.Mention} added a new Jack quote!"); // Append lines for the StringBuilder for Embed
            sb.AppendLine();
            sb.AppendLine($"Quote: '{quote}'");
            embed.Title = "New Jack quote!"; // Embed styles
            embed.Description = sb.ToString();
            embed.Color = new Color(0, 255, 0);

            await ReplyAsync(null, false, embed.Build()); // Build embed and reply to user with it

            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => {quote}"); // log to console

            LogToFile("!add", quote); // Log command
        }

        

        [Command("list")] // List all quotes in DB
        [Alias("l")]
        public async Task ListQuotes()
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            
            var user = Context.User;

            var quotes = await _db.JackBotQuotes.ToListAsync(); // Grab quotes from database and file as list
            List<string> quotesList = new List<string>();
            Char[] charArray = new Char[50000];

            int quoteAmount = 0;
            int wordAmount = 0;
            int charAmount = 0;

            if (quotes.Count > 0) // Make sure list isn't empty E.G Database isn't empty
            {
                
                foreach(var quote in quotes)
                {
                    quotesList.Add(quote.Quote); // Add quote to list because the previous one is a .dbList
                }
            }
            else
            {
                sb.AppendLine("No quotes here!");
            }

            if(quotesList.Count > 0) // Create char count code
            {
                
                foreach (string str in quotesList)
                {
                    
                    foreach(char c in str)
                    {
                        charArray[charAmount] = c; // Add char to Character array
                        charAmount++; // add to char amount
                    }
                }
            }

            if(quotesList.Count > 0) // Create word count code
            {
                foreach(string str in quotesList)
                {
                    List<string> wordCount = str.Split(" ").ToList();
                    wordAmount += wordCount.Count;
                }
            }

            if(charAmount <= 1500) // Safeguard around the char limit of 2048 for an embed description
            {
                foreach(var quote in quotes)
                {
                    quoteAmount++;
                    sb.AppendLine($"**{quoteAmount}** - {quote.Quote}"); // Append line for each quote in the Database
                    embed.Title = $"All the quotes!\nCurrent number of quotes: {quotes.Count}\nCurrent number of words: {wordAmount}\nCurrent amount of characters: {charAmount}"; // Displays quoteCount, wordCount, and charCount
                }
            }
            else
            {
                sb.AppendLine($"Too many quotes!\nWe have reached the character limit!\nCameron is currently trying to find a way to fix this!");
                embed.Title = $"Error 404\nCurrent character total: {charAmount}";
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !list is full! Character limit reached");
            }
            
            embed.Description = sb.ToString();
            embed.Color = new Color(255, 0, 255);

            await ReplyAsync(null, false, embed.Build()); // reply to user and build embed

            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !list"); // log to console

            LogToFile("!list"); // log to file
        }

        [Command("quote")] // Requested user handed random quote
        [Alias("q")]
        public async Task quoteRandom()
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();


            var random = new Random();

            var quotes = await _db.JackBotQuotes.ToListAsync(); // grab quotes from database in list format

            var user = Context.User;
            var givenQuote = string.Empty;

            if(quotes.Count > 0) // Check quotes isn't empty
            {
                var reply = quotes[new Random().Next(quotes.Count)]; // set "reply" to random quote from the list

                sb.AppendLine($"{reply.Quote}");
                givenQuote = reply.Quote; // Grab quote from logging
            }
            else
            {
                sb.AppendLine($"No quotes here! :(\nAdd some with !add");
            }

            embed.Title = $"Jack said:";
            embed.Description = sb.ToString();
            int red = random.Next(0, 256); // Random colours for that extra JAZZ
            int green = random.Next(0, 256);
            int blue = random.Next(0, 256);

            embed.Color = new Color(red, green, blue); // build random colour

            await ReplyAsync(null, false, embed.Build()); // reply to user and build embed

            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !quote"); // log to console

            LogToFile("!quote", givenQuote); // log to file
        }

        [Command("Help")] // just a help command
        [Alias("help")]
        public async Task quoteHelp()
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User;

            sb.AppendLine("-!add/!a -> Add a quote!"); // Append all lines
            sb.AppendLine("-!list/!l -> List all quotes!");
            sb.AppendLine("-!quote/!q -> Get a random Jack phrase!");
            sb.AppendLine("-!spotify/!s -> Get Jack's spotify!");
            sb.AppendLine("-!meme/!m -> Get a spicy Jack meme!");
            sb.AppendLine("-!twitter -> Get JackBot's Twitter!");

            embed.Title = "Help from Jack";
            embed.Description = sb.ToString();
            embed.Color = new Color(255, 255, 0); 

            await ReplyAsync(null, false, embed.Build()); // build embed

            Console.Write($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !help"); // log to console

            LogToFile("!help"); // log to file
        }

        [Command("Spotify")] // just shows spotify link
        [Alias("spotify", "s")]
        public async Task spotifyShow()
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User;

            sb.AppendLine("=> https://spoti.fi/3j8ko0a"); // shortened URL for Spotify link

            embed.Title = "The tunes!";
            embed.Description = sb.ToString();
            embed.Color = new Color(0, 250, 242); // specific colour

            await ReplyAsync(null, false, embed.Build()); // build embed

            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !spotify"); // log to console

            LogToFile("!spotify"); // log the use to file
        }

        [Command("meme")]
        [Alias("m")]
        public async Task giveMeme()
        {
            var user = Context.User;

            var files = new DirectoryInfo(@"D:\JackFacebookMemes\").GetFiles();
            int index = new Random().Next(0, files.Length);
            string memePath = @"D:\JackFacebookMemes\" + files[index].Name;

            await Context.Channel.SendFileAsync(memePath, "");
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !meme");
            LogToFile("!meme", memePath);
        }

        [Command("addmeme")] // Adds a meme directly from Discord into the directory
        [Alias("am")] // Alias
        [RequireUserPermission(GuildPermission.MuteMembers)] // Requires user to have MuteMembers permission
        public async Task addMeme()
        {
            var sb = new StringBuilder();
            var title = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User;

            MessageReference reference = Context.Message.Reference; // Gets reference message

            int red = 0; // Colours
            int green = 0;

            if(reference != null) // Check if reference is null (needs to be a reply)
            {
                
                var messageid = (ulong)reference.MessageId; // Gets reference message ID
                var messageRef = await Context.Channel.GetMessageAsync(messageid);  // Fetches the message throug the messageId

                if (messageRef.Attachments.Any()) // Checks to see if there's an attachment on the message
                {
                    string imageUrl = messageRef.Attachments.FirstOrDefault().Url; // gets the image's url from Discord
                    var imageFileName = messageRef.Attachments.FirstOrDefault().Filename; // gets the attachment's filename as given by Discord
                    string filePath = @"D:\JackFacebookMemes\" + imageFileName; // Creates directory path with filename

                    if (imageUrl != null || imageFileName != null) // Make sure that the ImageUrl and ImageFileName are actually fetched
                    {
                        if (!(File.Exists(filePath)))
                        {
                            using (var client = new HttpClient()) // Open new httpclient
                            {
                                var stream = await client.GetStreamAsync(imageUrl); // Feed the stream the url from the client
                                
                                using (var filestream = File.Create(filePath)) // create new file at the dedicated location with Discord's filename
                                {
                                    stream.CopyTo(filestream);  // create file
                                    sb.AppendLine($"File name: {imageFileName}"); // embed styling
                                    sb.AppendLine("");
                                    sb.Append($"Created at: {DateTime.Now.ToString("HH:mm:ss")} by {user.Mention}"); // tag user with their handle
                                    title.AppendLine($"File Created!");
                                    red = 0;
                                    green = 255;
                                    LogToFile("!addmeme", filePath); // log to logfile
                                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme => {imageFileName}"); // log to console

                                    filestream.Close(); // close filestream

                                }
                            }
                        }
                        else
                        {
                            sb.AppendLine($"{imageFileName} already exists!"); // Embed styling for error
                            title.AppendLine("File not created");
                            red = 255;
                            green = 0;
                            LogToFile("!addmeme", "Failed => File already exists");
                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme => **File already exists!**");
                        }

                    }
                    else
                    {
                        sb.AppendLine("Cannot use this image"); // Embed styling for error
                        title.AppendLine("File not created");
                        red = 255;
                        green = 0;
                        LogToFile("!addmeme", "Failed => Cannot use this image");
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme => **Cannot use this image**");
                    }
                }
                else
                {
                    sb.AppendLine("No image!"); // Embed styling for error
                    title.AppendLine("File not created");
                    red = 255;
                    green = 0;
                    LogToFile("!addmeme", "Failed => No image");
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme => **No image**");
                }

            }
            else if(Context.Message != null) // check current user message is not null
            {
                if (Context.Message.Attachments.Any()) // check if current user message has any attachments
                {
                    // -------------------------------------------------------------------------------------------------------------------------------
                    // ********* ALL CODE FROM HERE DOWN IS THE SAME AS ABOVE JUST DUPLICATED FOR THE CURRENT USER MESSAGE RATHER THAN REPLY *********
                    // -------------------------------------------------------------------------------------------------------------------------------

                    string imageUrl = Context.Message.Attachments.FirstOrDefault().Url; 
                    string imageFileName = Context.Message.Attachments.FirstOrDefault().Filename;
                    string filePath = @"D:\JackFacebookMemes\" + imageFileName;

                    if(imageFileName != null || imageUrl != null)
                    {
                        if (!(File.Exists(filePath)))
                        {
                            using (var client = new HttpClient())
                            {
                                var stream = await client.GetStreamAsync(imageUrl);

                                using (var filestream = File.Create(filePath))
                                {
                                    stream.CopyTo(filestream);

                                    sb.AppendLine($"File name: {imageFileName}"); // embed styling
                                    sb.AppendLine("");
                                    sb.Append($"Created at: {DateTime.Now.ToString("HH:mm:ss")} by {user.Mention}"); // tag user with their handle
                                    title.AppendLine($"File Created!");
                                    red = 0;
                                    green = 255;
                                    LogToFile("!addmeme", filePath); // log to logfile
                                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme => {imageFileName}"); // log to console

                                    filestream.Close(); // close filestream
                                }
                            }
                        }
                        else
                        {
                            sb.AppendLine($"{imageFileName} already exists!"); // Embed styling for error
                            title.AppendLine("File not created");
                            red = 255;
                            green = 0;
                            LogToFile("!addmeme", "Failed => File already exists");
                        }
                    }
                    else
                    {
                        sb.AppendLine("Cannot use this image"); // Embed styling for error
                        title.AppendLine("File not created");
                        red = 255;
                        green = 0;
                        LogToFile("!addmeme", "Failed => Cannot use this image");
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme");
                    }
                }
                else
                {
                    sb.AppendLine("No image!"); // Embed styling for error
                    title.AppendLine("File not created");
                    red = 255;
                    green = 0;
                    LogToFile("!addmeme", "Failed => No image");
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme");
                }
            }
            embed.Title = title.ToString(); // embed creation
            embed.Description = sb.ToString();
            embed.Color = new Color(red, green, 0);

            await ReplyAsync(null, false, embed.Build()); // Build embed and reply to user with it
        }

        [Command("twitter")]
        [Alias("t")]
        public async Task getTwitter()
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User.Username;

            sb.AppendLine("https://twitter.com/ketposting");
            embed.Title = "Jackbot Twitter - ";
            embed.Description = sb.ToString();
            embed.Color = new Color(0, 0, 255);

            await ReplyAsync(null, false, embed.Build());
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user} => !twitter");
            LogToFile("!twitter");
        }
    }
}