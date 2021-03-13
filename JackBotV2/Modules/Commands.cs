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
using JackBotV2.FOAAS;
using Newtonsoft.Json;

namespace JackBotV2.Modules
{
    public class Commands : ModuleBase
    {
        private readonly JackBotEntities _db; // set _db for later use in IServiceProvider
        private DiscordSocketClient _client;
        private static string streamPath = @"D:\JackbotLogs\JackBotLog.txt"; // Specify path to log file
        private const string pool = "0123456789";

        public Commands(IServiceProvider services) // Give _db the service it requires to hook into JackBotEntities 
        {
            _db = services.GetRequiredService<JackBotEntities>();
            var client = services.GetRequiredService<DiscordSocketClient>();
            _client = client;

            API_Stuff.APIHelper.InitialiseClient();
        }

        public static void LogToFile(string str, string user, string args = null) // Logging commands that are done via a .txt format 
        {
            using (StreamWriter sw = new StreamWriter(streamPath, true))
            {
                sw.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {user} => {str}: {args}");
            }
        }

        public static async Task<FOAASModel> GetFOAAS(string name, string from)
        {
            var rnd = new Random();
            var endpoint = "https://foaas.com";
            List<string> urls = new List<string>()
            {
                $"/asshole/{from}",
                $"/back/{name}/{from}",
                $"/bag/{from}",
                $"/because/{from}",
                $"/blackadder/{name}/{from}",
                $"/bucket/{from}",
                $"/bus/{name}/{from}",
                $"/chainsaw/{name}/{from}",
                $"/cocksplat/{name}/{from}",
                $"/cup/{from}",
                $"/diabetes/{from}",
                $"/even/{from}",
                $"/family/{from}",
                $"/fascinating/{from}",
                $"/fewer/{name}/{from}",
                $"/flying/{from}",
                $"/ftfy/{from}",
                $"/fyyff/{from}",
                $"/gfy/{name}/{from}",
                $"/give/{from}",
                $"/greed/poggies/{from}",
                $"/holygrail/{from}",
                $"/horse/{from}",
                $"/immensity/{from}",
                $"/ing/{name}/{from}",
                $"/jinglebells/{from}",
                $"/keep/{name}/{from}",
                $"/king/{name}/{from}",
                $"/linus/{name}/{from}",
                $"/looking/{from}",
                $"/madison/{name}/{from}",
                $"/maybe/{from}",
                $"/me/{from}",
                $"/nugget/{name}/{from}",
                $"/off/{name}/{from}",
                $"/outside/{name}/{from}",
                $"/problem/{name}/{from}",
                $"/question/{from}",
                $"/pulp/English/{from}",
                $"/shakespeare/{name}/{from}",
                $"/think/{name}/{from}",
                $"/too/{from}",
                $"/yoda/{name}/{from}",
                $"/zero/{from}",
                $"/you/{name}/{from}"
            };

            var index = rnd.Next(0, urls.Count);
            var fullURL = $"{endpoint}{urls[index].ToString()}";

            using (HttpResponseMessage response = await API_Stuff.APIHelper.APIClient.GetAsync(fullURL))
            {
                if (response.IsSuccessStatusCode)
                {
                    FOAASModel foaas = await response.Content.ReadAsAsync<FOAASModel>();
                    return foaas;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        [Command("add")] // add command
        [Alias("a")] // give alias to command !a !add
        [RequireUserPermission(GuildPermission.MuteMembers)] // require permission (muteMembers is generally a higher level command not for everyone)
        public async Task AddQuote([Remainder] string quote = null)  // Starts the add quote
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            //var errorSb = new StringBuilder();
            //var errorEmbed = new EmbedBuilder();

            var user = Context.User; // Grab the user
            var userForDb = Context.User.ToString(); // DB can only have strings inputted

            if (string.IsNullOrEmpty(quote)) // Error checking
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

            LogToFile("!add", userForDb, quote); // Log command
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

                foreach (var quote in quotes)
                {
                    quotesList.Add(quote.Quote); // Add quote to list because the previous one is a .dbList
                }
            }
            else
            {
                sb.AppendLine("No quotes here!");
            }

            if (quotesList.Count > 0) // Create char count code
            {

                foreach (string str in quotesList)
                {

                    foreach (char c in str)
                    {
                        charArray[charAmount] = c; // Add char to Character array
                        charAmount++; // add to char amount
                    }
                }
            }

            if (quotesList.Count > 0) // Create word count code
            {
                foreach (string str in quotesList)
                {
                    List<string> wordCount = str.Split(" ").ToList();
                    wordAmount += wordCount.Count;
                }
            }

            if (charAmount <= 1500) // Safeguard around the char limit of 2048 for an embed description
            {
                foreach (var quote in quotes)
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

            LogToFile("!list", user.Username); // log to file
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

            if (quotes.Count > 0) // Check quotes isn't empty
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
            sb.AppendLine("-!randomrole/!rr -> Get a random League Role/Champ/Build");
            sb.AppendLine("-!addwaifu/!aw -> Add a waifu to the collection!");
            sb.AppendLine("-!waifu/!w -> get a random waifu");
            sb.AppendLine("-!listwaifu/!lw -> List all currently saved waifus");
            sb.AppendLine("-!who/!whowaifu -> Reply to a waifu and get their name!");
            sb.AppendLine("!fuckoff/!fo -> Tell someone to fuck off... politely?");

            embed.Title = "Help from Jack";
            embed.Description = sb.ToString();
            embed.Color = new Color(255, 255, 0);

            await ReplyAsync(null, false, embed.Build()); // build embed

            Console.Write($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !help"); // log to console

            LogToFile("!help", user.Username); // log to file
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

            LogToFile("!spotify", user.Username); // log the use to file
        }

        [Command("meme")]
        [Alias("m")]
        public async Task giveMeme()
        {
            var user = Context.User;

            var files = new DirectoryInfo(@"D:\JackbotLogs\JackFacebookMemes\").GetFiles();
            int index = new Random().Next(0, files.Length);
            string memePath = @"D:\JackbotLogs\JackFacebookMemes\" + files[index].Name;

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
            var fileNameRandom = new StringBuilder();
            var title = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User;
            var rnd = new Random();
            var poolBuilder = new StringBuilder();

            MessageReference reference = Context.Message.Reference; // Gets reference message

            int red = 0; // Colours
            int green = 0;

            if (reference != null) // Check if reference is null (needs to be a reply)
            {

                var messageid = (ulong)reference.MessageId; // Gets reference message ID
                var messageRef = await Context.Channel.GetMessageAsync(messageid);  // Fetches the message throug the messageId

                if (messageRef.Attachments.Any()) // Checks to see if there's an attachment on the message
                {
                    string imageUrl = messageRef.Attachments.FirstOrDefault().Url; // gets the image's url from Discord
                    string imageFileName = messageRef.Attachments.FirstOrDefault().Filename;

                    if(imageFileName == "unknown.jpg" || imageFileName == "unknown.png")
                    {
                        var randomString = new string(Enumerable.Repeat(pool, 50)
                            .Select(s => s[rnd.Next(s.Length)]).ToArray());

                        imageFileName = $"{randomString}.jpg";
                    }
                    // Creates directory path with filename

                    if (imageUrl != null || imageFileName != null) // Make sure that the ImageUrl and ImageFileName are actually fetched
                    {
                        string filePath = @"D:\JackbotLogs\JackFacebookMemes\" + imageFileName;

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
                                    LogToFile("!addmeme", user.Username, filePath); // log to logfile
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
                            LogToFile("!addmeme", user.Username, "Failed => File already exists");
                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme => **File already exists!**");
                        }

                    }
                    else
                    {
                        sb.AppendLine("Cannot use this image"); // Embed styling for error
                        title.AppendLine("File not created");
                        red = 255;
                        green = 0;
                        LogToFile("!addmeme", user.Username, "Failed => Cannot use this image");
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme => **Cannot use this image**");
                    }
                }
                else
                {
                    sb.AppendLine("No image!"); // Embed styling for error
                    title.AppendLine("File not created");
                    red = 255;
                    green = 0;
                    LogToFile("!addmeme", user.Username, "Failed => No image");
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme => **No image**");
                }

            }
            else if (Context.Message != null) // check current user message is not null
            {
                if (Context.Message.Attachments.Any()) // check if current user message has any attachments
                {
                    // -------------------------------------------------------------------------------------------------------------------------------
                    // ********* ALL CODE FROM HERE DOWN IS THE SAME AS ABOVE JUST DUPLICATED FOR THE CURRENT USER MESSAGE RATHER THAN REPLY *********
                    // -------------------------------------------------------------------------------------------------------------------------------

                    string imageUrl = Context.Message.Attachments.FirstOrDefault().Url;
                    string imageFileName = Context.Message.Attachments.FirstOrDefault().Filename;

                    if (imageFileName == "unknown.jpg" || imageFileName == "unknown.png")
                    {
                        var randomString = new string(Enumerable.Repeat(pool, 50)
                            .Select(s => s[rnd.Next(s.Length)]).ToArray());

                        imageFileName = $"{randomString}.jpg";
                    }

                    if (imageFileName != null || imageUrl != null)
                    {
                        string filePath = @"D:\JackbotLogs\JackFacebookMemes\" + imageFileName;

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
                                    LogToFile("!addmeme", user.Username, filePath); // log to logfile
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
                            LogToFile("!addmeme", user.Username, "Failed => File already exists");
                        }
                    }
                    else
                    {
                        sb.AppendLine("Cannot use this image"); // Embed styling for error
                        title.AppendLine("File not created");
                        red = 255;
                        green = 0;
                        LogToFile("!addmeme", user.Username, "Failed => Cannot use this image");
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addmeme");
                    }
                }
                else
                {
                    sb.AppendLine("No image!"); // Embed styling for error
                    title.AppendLine("File not created");
                    red = 255;
                    green = 0;
                    LogToFile("!addmeme", user.Username, "Failed => No image");
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
            LogToFile("!twitter", user);
        }

        [Command("status")]
        [Alias("test")]
        public async Task getStatus()
        {
            var embed = new EmbedBuilder();
            var user = Context.User.Username;

            embed.Title = "I finally bothered to get out of bed";
            embed.Color = new Color(0, 255, 0);

            await ReplyAsync(null, false, embed.Build());
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user} => !status");
            LogToFile("!status", user);

        }

        [Command("randomrole")]
        [Alias("rr")]
        public async Task getRandomRole()
        {
            var embed = new EmbedBuilder();
            var sb = new StringBuilder();
            var rnd = new Random();

            var user = Context.User.Username;

            var listType = new List<string> { 
                "AP (Any AP items)", 
                "AD (Any AD items)", 
                "Hybrid (Any Hybrid items)", 
                "Tank (Any tank items)", 
                "AD Glass cannon", 
                "AP Glass cannon",
                "AD Tank",
                "AP Tank",
                "Hybrid Tank",
                "Support",
                "Squishy Support",
                "Tank Support"
            };
            var typeIndex = rnd.Next(listType.Count);

            string champPath = @"D:\JackbotLogs\LeagueChampsList.txt";
            string[] champs = File.ReadAllLines(champPath);
            sb.AppendLine($"**Champion:** {champs[rnd.Next(champs.Length)]}");

            var roleList = new List<string> { "Top", "Jungle", "Mid", "ADC", "Support" };
            var index = rnd.Next(roleList.Count);
            sb.AppendLine($"**Role:** {roleList[index]}");

            sb.AppendLine($"**Build Type:** {listType[typeIndex]}");

            int red = rnd.Next(0, 256);
            int green = rnd.Next(0, 256);
            int blue = rnd.Next(0, 256);

            embed.Title = $"{user} should play...";
            embed.Description = sb.ToString();
            embed.Color = new Color(red, green, blue);

            await ReplyAsync(null, false, embed.Build());
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user} => !randomrole");
            LogToFile("!randomrole", user);
        }

        [Command("dingle")]
        public async Task dingle()
        {
            var embed = new EmbedBuilder();
            var sb = new StringBuilder();
            var rnd = new Random();
            var user = Context.User.Username;
            int red;
            int blue;
            int green;

            sb.Append("dangle");
            red = rnd.Next(0, 256);
            blue = rnd.Next(0, 256);
            green = rnd.Next(0, 256);

            embed.Description = sb.ToString();
            embed.Color = new Color(red, green, blue);
            
            

            await ReplyAsync(null, false, embed.Build());
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user} => !status");
            LogToFile("!dingle", user);
        }

        [Command("addwaifu")]
        [Alias("aw")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task addWaifu([Remainder] string initialName = null)
        {
            var sb = new StringBuilder();
            var fileNameRandom = new StringBuilder();
            var title = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User;
            var rnd = new Random();
            var poolBuilder = new StringBuilder();
            
            List<string> cryingAnimeGirls = new List<string> { "https://media4.giphy.com/media/ROF8OQvDmxytW/200.gif",
            "https://media.tenor.com/images/7e623e17dd8c776aee5e0c3e0e9534c9/tenor.gif",
            "https://64.media.tumblr.com/5b4e0848d8080db04dbfedf31a4869e2/tumblr_nq1t6uTqRq1qcsnnso1_540.gif",
            "https://i.gifer.com/V2Kw.gif"
            };
            int index = rnd.Next(0, cryingAnimeGirls.Count);
            var errorImage = string.Empty;

            MessageReference reference = Context.Message.Reference; // Gets reference message

            int red = 0; // Colours
            int green = 0;

            if (!(string.IsNullOrEmpty(initialName)))
            {
                string name = initialName.ToLower();
                if (reference != null) // Check if reference is null (needs to be a reply)
                {

                    var messageid = (ulong)reference.MessageId; // Gets reference message ID
                    var messageRef = await Context.Channel.GetMessageAsync(messageid);  // Fetches the message throug the messageId

                    if (messageRef.Attachments.Any()) // Checks to see if there's an attachment on the message
                    {
                        string imageUrl = messageRef.Attachments.FirstOrDefault().Url; // gets the image's url from Discord
                        var imageFileName = messageRef.Attachments.FirstOrDefault().Filename;

                        if (imageFileName == "unknown.jpg" || imageFileName == "unknown.png")
                        {
                            var randomString = new string(Enumerable.Repeat(pool, 50)
                                .Select(s => s[rnd.Next(s.Length)]).ToArray());

                            imageFileName = $"{randomString}.jpg";
                        }

                        var newDirectoryName = @"D:\JackbotLogs\JackWaifus\" + name;
                        if (Directory.Exists(newDirectoryName))
                        {
                            if (imageUrl != null || imageFileName != null) // Make sure that the ImageUrl and ImageFileName are actually fetched
                            {

                                string filePath = $"{newDirectoryName}\\{imageFileName}";

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
                                            LogToFile("!addwaifu", user.Username, filePath); // log to logfile
                                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => {imageFileName}"); // log to console

                                            filestream.Close(); // close filestream

                                        }
                                    }
                                }
                                else
                                {
                                    sb.AppendLine($"{imageFileName} already exists!"); // Embed styling for error
                                    title.AppendLine("File not created");
                                    errorImage = cryingAnimeGirls[index].ToString();
                                    red = 255;
                                    green = 0;
                                    LogToFile("!addwaifu", user.Username, "Failed => File already exists");
                                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **File already exists!**");
                                }

                            }
                            else
                            {
                                sb.AppendLine("Cannot use this image"); // Embed styling for error
                                title.AppendLine("File not created");
                                errorImage = cryingAnimeGirls[index].ToString();
                                red = 255;
                                green = 0;
                                LogToFile("!addwaifu", user.Username, "Failed => Cannot use this image");
                                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **Cannot use this image**");
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(newDirectoryName);
                            if (imageUrl != null || imageFileName != null) // Make sure that the ImageUrl and ImageFileName are actually fetched
                            {
                                if (imageFileName == "unknown.jpg" || imageFileName == "unknown.png")
                                {
                                    var randomString = new string(Enumerable.Repeat(pool, 50)
                                        .Select(s => s[rnd.Next(s.Length)]).ToArray());

                                    imageFileName = $"{randomString}.jpg";
                                }
                                string filePath = $"{newDirectoryName}\\{imageFileName}";

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
                                            LogToFile("!addwaifu", user.Username, filePath); // log to logfile
                                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => {imageFileName}"); // log to console

                                            filestream.Close(); // close filestream

                                        }
                                    }
                                }
                                else
                                {
                                    sb.AppendLine($"{imageFileName} already exists!"); // Embed styling for error
                                    title.AppendLine("File not created");
                                    errorImage = cryingAnimeGirls[index].ToString();
                                    red = 255;
                                    green = 0;
                                    LogToFile("!addwaifu", user.Username, "Failed => File already exists");
                                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **File already exists!**");
                                }
                            }
                        }
                    }
                    else
                    {
                        sb.AppendLine("No image!"); // Embed styling for error
                        title.AppendLine("File not created");
                        errorImage = cryingAnimeGirls[index].ToString();
                        red = 255;
                        green = 0;
                        LogToFile("!addwaifu", user.Username, "Failed => No image");
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **No image**");
                    }

                }
                else if (Context.Message != null) // check current user message is not null
                {
                    if (Context.Message.Attachments.Any()) // check if current user message has any attachments
                    {
                        // -------------------------------------------------------------------------------------------------------------------------------
                        // ********* ALL CODE FROM HERE DOWN IS THE SAME AS ABOVE JUST DUPLICATED FOR THE CURRENT USER MESSAGE RATHER THAN REPLY *********
                        // -------------------------------------------------------------------------------------------------------------------------------
                        string imageUrl = Context.Message.Attachments.FirstOrDefault().Url;
                        string imageFileName = Context.Message.Attachments.FirstOrDefault().Filename;

                        if (imageFileName == "unknown.jpg" || imageFileName == "unknown.png")
                        {
                            var randomString = new string(Enumerable.Repeat(pool, 50)
                                .Select(s => s[rnd.Next(s.Length)]).ToArray());

                            imageFileName = $"{randomString}.jpg";
                        }

                        var newDirectoryName = @"D:\JackbotLogs\JackWaifus\" + name;

                        if (Directory.Exists(newDirectoryName))
                        {
                            if (imageUrl != null || imageFileName != null) // Make sure that the ImageUrl and ImageFileName are actually fetched
                            {
                                string filePath = $"{newDirectoryName}\\{imageFileName}";

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
                                            LogToFile("!addwaifu", user.Username, filePath); // log to logfile
                                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => {imageFileName}"); // log to console

                                            filestream.Close(); // close filestream

                                        }
                                    }
                                }
                                else
                                {
                                    sb.AppendLine($"{imageFileName} already exists!"); // Embed styling for error
                                    title.AppendLine("File not created");
                                    errorImage = cryingAnimeGirls[index].ToString();
                                    red = 255;
                                    green = 0;
                                    LogToFile("!addwaifu", user.Username, "Failed => File already exists");
                                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **File already exists!**");
                                }

                            }
                            else
                            {
                                sb.AppendLine("Cannot use this image"); // Embed styling for error
                                title.AppendLine("File not created");
                                errorImage = cryingAnimeGirls[index].ToString();
                                red = 255;
                                green = 0;
                                LogToFile("!addwaifu", user.Username, "Failed => Cannot use this image");
                                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **Cannot use this image**");
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(newDirectoryName);
                            if (imageUrl != null || imageFileName != null) // Make sure that the ImageUrl and ImageFileName are actually fetched
                            {
                                if (imageFileName == "unknown.jpg" || imageFileName == "unknown.png")
                                {
                                    var randomString = new string(Enumerable.Repeat(pool, 50)
                                        .Select(s => s[rnd.Next(s.Length)]).ToArray());

                                    imageFileName = $"{randomString}.jpg";
                                }

                                string filePath = $"{newDirectoryName}\\{imageFileName}";

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
                                            LogToFile("!addwaifu", user.Username, filePath); // log to logfile
                                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => {imageFileName}"); // log to console

                                            filestream.Close(); // close filestream

                                        }
                                    }
                                }
                                else
                                {
                                    sb.AppendLine($"{imageFileName} already exists!"); // Embed styling for error
                                    title.AppendLine("File not created");
                                    errorImage = cryingAnimeGirls[index].ToString();
                                    red = 255;
                                    green = 0;
                                    LogToFile("!addwaifu", user.Username, "Failed => File already exists");
                                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **File already exists!**");
                                }
                            }
                        }

                    }
                    else
                    {
                        sb.AppendLine("No image!"); // Embed styling for error
                        title.AppendLine("File not created");
                        errorImage = cryingAnimeGirls[index].ToString();
                        red = 255;
                        green = 0;
                        LogToFile("!addwaifu", user.Username, "Failed => No image");
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **No image**");
                    }
                }
            }
            else
            {
                // Code for no name waifu btw

                if (reference != null) // Check if reference is null (needs to be a reply)
                {

                    var messageid = (ulong)reference.MessageId; // Gets reference message ID
                    var messageRef = await Context.Channel.GetMessageAsync(messageid);  // Fetches the message throug the messageId

                    if (messageRef.Attachments.Any()) // Checks to see if there's an attachment on the message
                    {
                        string imageUrl = messageRef.Attachments.FirstOrDefault().Url; // gets the image's url from Discord
                        var imageFileName = messageRef.Attachments.FirstOrDefault().Filename;

                        if (imageFileName == "unknown.jpg" || imageFileName == "unknown.png")
                        {
                            var randomString = new string(Enumerable.Repeat(pool, 50)
                                .Select(s => s[rnd.Next(s.Length)]).ToArray());

                            imageFileName = $"{randomString}.jpg";
                        }

                        var newDirectoryName = @"D:\JackbotLogs\JackWaifus\unknown";
                        if (Directory.Exists(newDirectoryName))
                        {
                            if (imageUrl != null || imageFileName != null) // Make sure that the ImageUrl and ImageFileName are actually fetched
                            {

                                string filePath = $"{newDirectoryName}\\{imageFileName}";

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
                                            LogToFile("!addwaifu", user.Username, filePath); // log to logfile
                                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => {imageFileName}"); // log to console

                                            filestream.Close(); // close filestream

                                        }
                                    }
                                }
                                else
                                {
                                    sb.AppendLine($"{imageFileName} already exists!"); // Embed styling for error
                                    title.AppendLine("File not created");
                                    errorImage = cryingAnimeGirls[index].ToString();
                                    red = 255;
                                    green = 0;
                                    LogToFile("!addwaifu", user.Username, "Failed => File already exists");
                                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **File already exists!**");
                                }

                            }
                            else
                            {
                                sb.AppendLine("Cannot use this image"); // Embed styling for error
                                title.AppendLine("File not created");
                                errorImage = cryingAnimeGirls[index].ToString();
                                red = 255;
                                green = 0;
                                LogToFile("!addwaifu", user.Username, "Failed => Cannot use this image");
                                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **Cannot use this image**");
                            }
                        }
                    }
                }
                else if (Context.Message != null) // check current user message is not null
                {
                    if (Context.Message.Attachments.Any()) // check if current user message has any attachments
                    {
                        // -------------------------------------------------------------------------------------------------------------------------------
                        // ********* ALL CODE FROM HERE DOWN IS THE SAME AS ABOVE JUST DUPLICATED FOR THE CURRENT USER MESSAGE RATHER THAN REPLY *********
                        // -------------------------------------------------------------------------------------------------------------------------------

                        string imageUrl = Context.Message.Attachments.FirstOrDefault().Url;
                        string imageFileName = Context.Message.Attachments.FirstOrDefault().Filename;

                        if (imageFileName == "unknown.jpg" || imageFileName == "unknown.png")
                        {
                            var randomString = new string(Enumerable.Repeat(pool, 50)
                                .Select(s => s[rnd.Next(s.Length)]).ToArray());

                            imageFileName = $"{randomString}.jpg";
                        }

                        var newDirectoryName = @"D:\JackbotLogs\JackWaifus\unkown";

                        if (Directory.Exists(newDirectoryName))
                        {
                            if (imageUrl != null || imageFileName != null) // Make sure that the ImageUrl and ImageFileName are actually fetched
                            {
                                string filePath = $"{newDirectoryName}\\{imageFileName}";

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
                                            LogToFile("!addwaifu", user.Username, filePath); // log to logfile
                                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => {imageFileName}"); // log to console

                                            filestream.Close(); // close filestream

                                        }
                                    }
                                }
                                else
                                {
                                    sb.AppendLine($"{imageFileName} already exists!"); // Embed styling for error
                                    title.AppendLine("File not created");
                                    errorImage = cryingAnimeGirls[index].ToString();
                                    red = 255;
                                    green = 0;
                                    LogToFile("!addwaifu", user.Username, "Failed => File already exists");
                                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **File already exists!**");
                                }

                            }
                            else
                            {
                                sb.AppendLine("Cannot use this image"); // Embed styling for error
                                title.AppendLine("File not created");
                                errorImage = cryingAnimeGirls[index].ToString();
                                red = 255;
                                green = 0;
                                LogToFile("!addwaifu", user.Username, "Failed => Cannot use this image");
                                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **Cannot use this image**");
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(newDirectoryName);
                            if (imageUrl != null || imageFileName != null) // Make sure that the ImageUrl and ImageFileName are actually fetched
                            {
                                if (imageFileName == "unknown.jpg" || imageFileName == "unknown.png")
                                {
                                    var randomString = new string(Enumerable.Repeat(pool, 50)
                                        .Select(s => s[rnd.Next(s.Length)]).ToArray());

                                    imageFileName = $"{randomString}.jpg";
                                }

                                string filePath = $"{newDirectoryName}\\{imageFileName}";

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
                                            LogToFile("!addwaifu", user.Username, filePath); // log to logfile
                                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => {imageFileName}"); // log to console

                                            filestream.Close(); // close filestream

                                        }
                                    }
                                }
                                else
                                {
                                    sb.AppendLine($"{imageFileName} already exists!"); // Embed styling for error
                                    title.AppendLine("File not created");
                                    errorImage = cryingAnimeGirls[index].ToString();
                                    red = 255;
                                    green = 0;
                                    LogToFile("!addwaifu", user.Username, "Failed => File already exists");
                                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **File already exists!**");
                                }
                            }
                        }
                    }
                    else
                    {
                        sb.AppendLine("No image!"); // Embed styling for error
                        title.AppendLine("File not created");
                        errorImage = cryingAnimeGirls[index].ToString();
                        red = 255;
                        green = 0;
                        LogToFile("!addwaifu", user.Username, "Failed => No image");
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user.Username} => !addwaifu => **No image**");
                    }
                }
            }
            embed.Title = title.ToString(); // embed creation
            embed.Description = sb.ToString();
            embed.ImageUrl = errorImage;
            embed.Color = new Color(red, green, 0);

            await ReplyAsync(null, false, embed.Build()); // Build embed and reply to user with it
        }

        [Command("waifu")]
        [Alias("w")]
        public async Task getWaifu([Remainder]string initialName = null)
        {

            var user = Context.User.Username;
            var sb = new StringBuilder();
            var rnd = new Random();
            var embed = new EmbedBuilder();
            List<string> cryingAnimeGirls = new List<string> { "https://media4.giphy.com/media/ROF8OQvDmxytW/200.gif",
            "https://media.tenor.com/images/7e623e17dd8c776aee5e0c3e0e9534c9/tenor.gif",
            "https://64.media.tumblr.com/5b4e0848d8080db04dbfedf31a4869e2/tumblr_nq1t6uTqRq1qcsnnso1_540.gif",
            "https://i.gifer.com/V2Kw.gif"
            };
            int WaifuIndex = rnd.Next(0, cryingAnimeGirls.Count);
            var errorImage = string.Empty;


            if (!(string.IsNullOrEmpty(initialName)))
            {
                string name = initialName.ToLower();
                if (Directory.Exists($"D:\\JackbotLogs\\JackWaifus\\{name}"))
                {
                    var files = new DirectoryInfo($"D:\\JackbotLogs\\JackWaifus\\{name}").GetFiles();
                    int index = new Random().Next(0, files.Length);
                    string waifuPath = $"D:\\JackbotLogs\\JackWaifus\\{name}\\" + files[index].Name;

                    await Context.Channel.SendFileAsync(waifuPath, "");
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user} => !waifu");
                    LogToFile("!waifu", user, waifuPath);
                }
                else
                {
                    sb.AppendLine("Check your spelling, Manu");
                    embed.Title = "Error 404";
                    embed.Description = sb.ToString();
                    embed.ImageUrl = cryingAnimeGirls[WaifuIndex];
                    embed.Color = new Color(255, 0, 0);
                    embed.WithFooter(footer => footer.Text = "test");

                    await ReplyAsync(null, false, embed.Build());
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user} => !waifu => Error");
                    LogToFile("!waifu", user, "Error");
                }
                
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo($"D:\\JackbotLogs\\JackWaifus\\");
                DirectoryInfo[] diArray = di.GetDirectories();
                int index = rnd.Next(diArray.Length);

                List<string> allDirs = new List<string>();
                foreach (DirectoryInfo dri in diArray)
                {
                    allDirs.Add(dri.Name);
                }

                var randomDir = allDirs[index];
                if (Directory.Exists($"D:\\JackbotLogs\\JackWaifus\\{randomDir}"))
                {
                    var files = new DirectoryInfo($"D:\\JackbotLogs\\JackWaifus\\{randomDir}").GetFiles();
                    int randomWaifuIndex = rnd.Next(0, files.Length);
                    string waifuPath = $"D:\\JackbotLogs\\JackWaifus\\{randomDir}\\" + files[randomWaifuIndex].Name;

                    await Context.Channel.SendFileAsync(waifuPath, "");
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user} => !waifu");
                    LogToFile("!waifu", user, waifuPath);
                }
            }
        }

        [Command("listwaifu")]
        [Alias("lw")]
        public async Task listWaifus()
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User.Username;
            DirectoryInfo di = new DirectoryInfo($"D:\\JackbotLogs\\JackWaifus\\");
            DirectoryInfo[] diArray = di.GetDirectories();
            foreach (DirectoryInfo dri in diArray)
            {
                string nameOfFile = dri.Name;
                sb.AppendLine(char.ToUpper(nameOfFile[0]) + nameOfFile.Substring(1));
            }

            
            embed.Title = "All current Waifus";
            embed.Description = sb.ToString();
            embed.Color = new Color(255, 192, 203);

            await ReplyAsync(null, false, embed.Build());
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user} => !listwaifus");
            LogToFile("!waifu", user);
        }

        [Command("whowaifu")]
        [Alias("ww", "who")]
        public async Task whoWaifu()
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var rnd = new Random();
            var user = Context.User.Username;
            List<string> cryingAnimeGirls = new List<string> { "https://media4.giphy.com/media/ROF8OQvDmxytW/200.gif",
            "https://media.tenor.com/images/7e623e17dd8c776aee5e0c3e0e9534c9/tenor.gif",
            "https://64.media.tumblr.com/5b4e0848d8080db04dbfedf31a4869e2/tumblr_nq1t6uTqRq1qcsnnso1_540.gif",
            "https://i.gifer.com/V2Kw.gif"
            };
            int index = rnd.Next(0, cryingAnimeGirls.Count);
            var errorImage = string.Empty;

            MessageReference reference = Context.Message.Reference;

            if(reference != null)
            {
                var messageid = (ulong)reference.MessageId;
                var messageRef = await Context.Channel.GetMessageAsync(messageid);
                
                if (messageRef.Attachments.Any())
                {
                    var imageFileName = messageRef.Attachments.FirstOrDefault().Filename;
                    if(!(string.IsNullOrEmpty(imageFileName) || imageFileName == "unknown.png" || imageFileName == "unknown.jpg"))
                    {
                        string waifuPath = $"D:\\JackbotLogs\\JackWaifus\\";
                        List<string> folder = new DirectoryInfo(waifuPath)
                            .EnumerateFiles(imageFileName, SearchOption.AllDirectories)
                            .Select(d => d.FullName).ToList();

                        foreach (var w in folder)
                        {
                            FileInfo fInfo = new FileInfo(w);
                            string dirName = fInfo.Directory.Name;
                            sb.AppendLine($"This is **{char.ToUpper(dirName[0]) + dirName.Substring(1)}**!");

                        }
                    }
                    else
                    {
                        sb.AppendLine("No waifu here.... :(");
                        sb.AppendLine("This might be a screenshot?");
                        errorImage = cryingAnimeGirls[index];
                    }
                    
                }
                else
                {
                    sb.AppendLine("No waifu here.... :(");
                    errorImage = cryingAnimeGirls[index];

                }
            }
            else
            {
                sb.AppendLine("No waifu here.... :( ");
                errorImage = cryingAnimeGirls[index];
            }
            embed.Title = "Fetching the Waifu...";
            embed.Description = sb.ToString();
            embed.ImageUrl = errorImage;
            embed.Color = new Color(255, 192, 203);

            await ReplyAsync(null, false, embed.Build());
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} => {user} => !who");
            LogToFile("!who", user);
        }

        [Command("fuckoff")]
        [Alias("fo")]
        public async Task FOAAS([Remainder]string query)
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User.Username;
            var fo = await GetFOAAS(query, user);

            sb.AppendLine(fo.message);
            sb.AppendLine(fo.subtitle);

            embed.Title = "FOAAS";
            embed.Description = sb.ToString();
            embed.Color = new Color(255, 0, 0);

            await ReplyAsync(null, false, embed.Build());
        }
    }
}