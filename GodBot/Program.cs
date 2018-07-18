using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;

namespace GodBot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        //Constants 
        const string meeBot = "MEE6";
        HashSet<String> roles = new HashSet<string>();
        IFirebaseClient firebaseClient;
        IFirebaseConfig firebaseConfig;

        public async Task MainAsync()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var config = new DiscordSocketConfig { MessageCacheSize = 100 };
            var client = new DiscordSocketClient(config);
            string discordToken;
            string authSecret;
            string basePath;

            StreamReader file = new StreamReader(@"tokens.txt");
            discordToken = file.ReadLine();
            authSecret = file.ReadLine();
            basePath = file.ReadLine();

            firebaseConfig = new FirebaseConfig
            {
                AuthSecret = authSecret,
                BasePath = basePath,
            };

            firebaseClient = new FireSharp.FirebaseClient(firebaseConfig);

            roles.Add("Dota 2");
            roles.Add("CS:GO");
            roles.Add("PUBG");
            roles.Add("HearthStone");
            roles.Add("WoW");
            roles.Add("Fortnite");
            roles.Add("GTA V");

            await client.LoginAsync(TokenType.Bot, discordToken);
            await client.StartAsync();

            //client.Log += Log;
            client.MessageReceived += MessageReceived;
            client.MessageUpdated += MessageUpdated;
            client.MessageDeleted += MessageDeleted;

            Firebase firebase = new Firebase();
            firebase.initFirebase(authSecret, basePath);

            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            SocketUser user = message.Author;

            if (message.Content == "!flip")
            {
                Random rand = new Random();
                if (rand.Next(0, 100) > 50)
                    await message.Channel.SendMessageAsync(":+1:");
                else await message.Channel.SendMessageAsync(":-1:");
            }

            string[] tokens = message.Content.Split(' ');
            string role = "";
            foreach (var token in tokens)
            {
                if (token != tokens[0])
                {
                    if (role != "") role += " ";
                    role += token;
                }
            }

            if (tokens[0] == "!addrole")
            {
                if (roles.Contains(role))
                {
                    var user1 = user as SocketGuildUser;
                    var userRole = (user1 as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == role);
                    
                    if (user1.Roles.Contains(userRole))
                        await user1.RemoveRoleAsync(userRole);
                    else await user1.AddRoleAsync(userRole);

                }
            }

            if (tokens[0] == "!removerole")
            {
                var user1 = user as SocketGuildUser;
                var userRole = (user1 as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == role);
                if (user1.Roles.Contains(userRole))
                    await user1.RemoveRoleAsync(userRole);
                //await message.Channel.SendMessageAsync("This role doesn't exist <:FeelsBadMan:456407012243275787>");
            }

            if (user.Username == meeBot)
            {
                if (message.Content.Equals(""))
                {
                    //foreach (var ment in message.MentionedUsers)
                    //{

                    //}
                    System.Threading.Thread.Sleep(10000);
                    await message.DeleteAsync();
                }
            }

            string attachment = "";
            foreach (var a in message.Attachments)
            {
                attachment = a.Url;
            }
            Console.WriteLine($"{message.Timestamp.DateTime} [{message.Channel}] {message.Author}: {message} {attachment}");


            if (message.Content.ToString()[0] == '!')
            {
                //Create new thread
                System.Threading.Thread.Sleep(10000);
                await message.DeleteAsync();
            }
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message.Timestamp.DateTime} [{message.Channel}] {message.Author} {message} -> {after} (eddited)");
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> before, ISocketMessageChannel channel)
        {
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message.Timestamp.DateTime} [{message.Channel}] {message.Author} {message} (deleted)");
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}