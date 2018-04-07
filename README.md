# Redditizer
A discord bot that parses and links to Subreddits

![example](https://i.imgur.com/sUBokwO.png)

## About
This is a bot for Discord written in C# .NET 4.6.1, using the [RedditSharp](https://github.com/CrustyJew/RedditSharp) and [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) libraries for the Reddit and Discord APIs, respectively. Once this bot is set up, running, and added to your discord server, it will passively read comments and if they contain a subreddit (ex /r/aww or r/news), it will parse that out and send an Embed with a link to that subreddit and it's description and the [top 3 posts](https://i.imgur.com/HjIHDtk.png) if you choose.

## Usage
Type any subreddit in either by itself or in a sentence, and the bot will detect it. If you want to also display that subreddit's top 3 posts of all time, you can add a flag `-t`, `-top`, or `-top3` to also have the bot print out those posts. 

## Configuration and setup
Configure the bot to log into Reddit:
1. You will need to set up a personal bot on your reddit account. Either with a new account or your personal one, go to https://www.reddit.com/prefs/apps/, and click the button at the bottom to create an app. 
2. Give the bot a name, and select the option to create it as a script. Leave the `about url` blank and the `redirect uri` set to `http://127.0.0.1`. Click create app.
3. Once created you'll be presented with a ClientID and a ClientSecret (the ID is the random string next to the Icon, and the secret is labelled below). Copy both of these these as well as the account's username and password into the app.config (Redditizer.exe.config).

Configure the bot to connect to your Discord Server (taken/modified from the [DSharpPlus documentation](https://github.com/DSharpPlus/Example-Bots/blob/master/README.md)):
1. Go to [my applications](https://discordapp.com/developers/applications/me) page on Discord Developer portal.
2. Press the [**new app** button](http://i.imgur.com/IVsPyNw.png).
3. [**New app** page](http://i.imgur.com/3mrEG9x.png) will open. Enter your bot's name in the **app name** field (1), and its description in the **description** field (2).
   * You can optionally give it an avatar by pressing on the **avatar** button (3) (You can use the included `Resources\redditizer icon.png` if you'd like).
4. When you're done, press the [**create app** button](http://i.imgur.com/ur3HFng.png).
5. When the app is created, press the [**create bot user** button](http://i.imgur.com/b69CHy7.png).
6. Once this is done, you will need to copy the **bot's token**. Under **app bot user**, there's a **token** field, press [**click to reveal**](http://i.imgur.com/00b4Nt8.png) and copy the resulting value.
7. Add this value into the app.config (Redditizer.exe.config) for the `DiscordToken` key.
8. Go back to your app page, and copy your bot's [**client ID**](http://i.imgur.com/NuAPpoY.png).
9. Go to `https://discordapp.com/oauth2/authorize?client_id=your_app_client_id&scope=bot&permissions=0`.
10. On the [page](http://i.imgur.com/QeH0o5S.png), select **your server** (1), and press **authorize** (2).
11. [Done](http://i.imgur.com/LF1gpm2.png)! 

After the requisite data is in the config file, and you have a bot registered with Reddit and Discord, you can run the Redditizer.exe (or run from the VS Debugger). You should see the [console window open](https://i.imgur.com/8hL35JQ.png) with sucessful messages connecting to reddit, initalizing Discord, and a heartbeat. 
