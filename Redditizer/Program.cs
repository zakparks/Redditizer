using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using DSharpPlus;
using DSharpPlus.EventArgs;
using RedditSharp;
using RedditSharp.Things;
using System.IO;
using Newtonsoft.Json;
using DSharpPlus.Entities;
using System.Diagnostics.Contracts;
using System.Collections.Specialized;

namespace Redditizer
{
    class Program
    {
        // app settings
        public static NameValueCollection appSettings;

        // reddit login information
        public static BotWebAgent WebAgent;
        public static Reddit RedditInstance;
        public static Subreddit Subreddit;

        // Virtual Discord client
        public DiscordClient Client { get; set; }

        // entry point for the application
        static void Main(string[] args)
        {
            var prog = new Program();
            appSettings = ConfigurationManager.AppSettings;
            prog.RunBotAsync().GetAwaiter().GetResult();
        }

        // main task that listens for the incoming messages
        public async Task RunBotAsync()
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            try
            {
                // setup discord configuration
                DiscordConfiguration cfg = new DiscordConfiguration
                {
                    Token = appSettings["DiscordToken"],
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    LogLevel = LogLevel.Debug,
                    UseInternalLogHandler = true
                };

                // instantiate the client
                Client = new DiscordClient(cfg);

                // Hook into client events, so we know what's going on
                Client.Ready += Client_Ready;
                Client.ClientErrored += Client_ClientError;
                Client.MessageCreated += Client_MessageCreated;
            }
            catch (Exception e)
            {
                Client.DebugLogger.LogMessage(LogLevel.Error, "Redditizer", "Error initializing Discord client:" + e.Message, DateTime.Now);
            }

            try
            {
                // set up the reddit user account using OAuth
                
                string username = appSettings["RedditUsername"] ?? null;
                string pwd = appSettings["RedditPassword"] ?? null;
                if (username == null || pwd == null) { throw new Exception("Username or password was null in Redditizer.exe.config."); }

                string clientID = appSettings["ClientID"] ?? null;
                string clientSecret = appSettings["ClientSecret"] ?? null;
                if (clientID == null || clientSecret == null) { throw new Exception("ClientID or ClientSecret was null in Redditizer.exe.config."); }

                WebAgent = new BotWebAgent(username, pwd, clientID, clientSecret, "");
                RedditInstance = new Reddit(WebAgent);

                //RedditInstance = new Reddit(username, pwd, true);
                Client.DebugLogger.LogMessage(LogLevel.Info, "Redditizer", "Reddit logged in", DateTime.Now);
            }
            catch (Exception e)
            {
                Client.DebugLogger.LogMessage(LogLevel.Error, "Redditizer", "Error logging into Reddit:" + e.Message, DateTime.Now);
            }

            //connect and log in
            await Client.ConnectAsync();

            //prevent premature quitting
            await Task.Delay(-1);
        }

        /// <summary>
        /// intercept messages and reply to them if applicable
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            // ignore bot comments
            if (e.Author.IsBot) return;

            // listener for messages, to see if someone mentions a subreddit
            try
            {
                string msg = e.Message.Content.ToLower();
                // only accept comments that contain a subreddit tag, but not a full url
                if ((msg.Contains("/r/") || msg.Contains("r/")) && !msg.Contains("reddit.com"))
                {
                    // pass the comment containing the subreddit mention into the processing method
                    DiscordEmbed reply = BuildReply(msg);

                    // failsafe to prevent bad data being printed into the discord chat.
                    if (reply != null)
                    {
                        // print the message as a bot comment
                        await e.Message.RespondAsync("", embed: reply);
                        Client.DebugLogger.LogMessage(LogLevel.Info, "Redditizer", "Embed sent", DateTime.Now);
                    }
                }
            }
            catch (Exception ex)
            {
                Client.DebugLogger.LogMessage(LogLevel.Error, "Redditizer", "Error with incoming message while trying to parse a subreddit: " + ex.Message, DateTime.Now);
            }
        }

        /// <summary>
        /// Return a status when the client is ready
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task Client_Ready(ReadyEventArgs e)
        {
            // log the fact that this event occured
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Redditizer", "Client is ready to process events.", DateTime.Now);

            // since this method is not async, return a completed task
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns an error message when the Client errors out for whatever reason
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            // log the details of the error that just occured in our client
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "Redditizer", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            // since this method is not async, return a completed task
            return Task.CompletedTask;
        }

        /// <summary>
        /// Parse a verified comment's subreddit mention out, and get information about it to print back out
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DiscordEmbed BuildReply(string input)
        {
            string sub = null;
            bool showTop3 = false;
            string baseURL = "https://reddit.com";
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            //parse out the subreddit, and get any arguments
            var strs = input.Split(' ');
            foreach (string s in strs)
            {
                if (s.Contains("r/")) sub = s;

                // flag to show the top 3 posts
                if (new[] { "-top3", "-top", "-t"}.Contains(s))
                {
                    showTop3 = true;
                }
            }
            
            //check if subreddit exists
            try
            {
                Subreddit = RedditInstance.GetSubreddit(sub);
            }
            catch
            {
                Console.WriteLine("Error getting subreddit/subreddit doesn't exist");
                return null;
            }

            // build the simple embed that links only the subreddit
            embedBuilder.Author = new DiscordEmbedBuilder.EmbedAuthor { Name = Subreddit.Name, Url = baseURL + Subreddit.Url };

            // some subreddits have a ton of extra crap or are wordy in their public description. Trim it down if its too big. 
            // 185 characters seems to be a good balance to keep this at 3 lines, which is the height of the image.
            embedBuilder.Description = Subreddit.PublicDescription.Length < 185 ? Subreddit.PublicDescription.Replace('\n',' ') : Subreddit.PublicDescription.Substring(0,185).Replace('\n', ' ') + "...";
            embedBuilder.Url = baseURL + Subreddit.Url;
            embedBuilder.ThumbnailUrl = Subreddit.NSFW ? @"https://i.imgur.com/UtJLo7A.png" : @"https://i.imgur.com/xyZgMs0.png";
            
            // build the full embed with the top 3 of all time
            if (showTop3)
            {
                // get the URL, top 3 posts, and format their title and permalink into a comment
                var top3 = Subreddit.GetTop(FromTime.All).Take(3).ToList();
                                
                for (int i = 0; i < 3; i++)
                {
                    if (top3[i].NSFW)
                    {
                        embedBuilder.AddField((i + 1) + ". *NSFW*: " + top3[i].Title + "\n", baseURL + top3[i].Permalink, false);
                        embedBuilder.ThumbnailUrl = @"https://i.imgur.com/UtJLo7A.png";
                    }
                    else
                    {
                        embedBuilder.AddField((i + 1) + ". " + top3[i].Title, baseURL + top3[i].Permalink, false);
                    }
                }
            }

            // return the block of text to be printed out by the bot in chat
            return embedBuilder.Build();
        }
    }
}
