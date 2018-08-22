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

namespace GodBot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        //Constants 
        const string meeBot = "MEE6";
        const string mainUsersPath = "Users";
        float userRate = 1;
        HashSet<String> roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        IFirebaseClient firebaseClient;
        IFirebaseConfig firebaseConfig;
        Firebase firebase = new Firebase();
        

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

            //firebaseClient = new FireSharp.FirebaseClient(firebaseConfig);
            firebase.initFirebase(authSecret, basePath);

            roles.Add("Dota 2");
            roles.Add("CS:GO");
            roles.Add("PUBG");
            roles.Add("HearthStone");
            roles.Add("WoW");
            roles.Add("Fortnite");
            roles.Add("GTA V");
            roles.Add("LoL");
            roles.Add("PoE");
            roles.Add("Civilization");
            roles.Add("OSU");
            roles.Add("R6S");
            roles.Add("Diablo");
            roles.Add("WoT");
            roles.Add("Player");

            await client.LoginAsync(TokenType.Bot, discordToken);
            await client.StartAsync();

            //client.Log += Log;
            client.MessageReceived += MessageReceived;
            client.MessageUpdated += MessageUpdated;
            client.MessageDeleted += MessageDeleted;
            client.UserVoiceStateUpdated += UserVoiceStateUpdated;

            await Task.Delay(-1);
        }

        private async Task UserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState socketVoiceState, SocketVoiceState userVoiceStateUpdated)
        {
            var userInfo = await firebase.getSingleUser(mainUsersPath, socketUser.Id);
            SocketGuildUser guildUser = socketUser as SocketGuildUser;

            if (userInfo == null)
            {
                userInfo = new Coins();
                userInfo.userId = socketUser.Id;
                userInfo.coinsCount = 100;
                userInfo.joinedVoice = DateTime.UtcNow;
                userInfo.userRate = 0.5f;
                await firebase.setData(mainUsersPath, socketUser.Id.ToString(), userInfo);
                return;
            }

            if (userVoiceStateUpdated.VoiceChannel != null)
            {
                if (userVoiceStateUpdated.VoiceChannel.Name == "AFK♿")
                {
                    if (socketVoiceState.VoiceChannel != null)
                    {
                        if (socketVoiceState.VoiceChannel.Name != "AFK♿")
                        {
                            if (guildUser.IsDeafened || guildUser.IsSelfDeafened)
                                return;
                            TimeSpan result = DateTime.UtcNow.Subtract(userInfo.joinedVoice);
                            int seconds = Convert.ToInt32(result.TotalSeconds);
                            var coinsSum = userInfo.coinsCount + seconds;

                            userInfo.joinedVoice = DateTime.UtcNow;
                            userInfo.coinsCount = coinsSum;
                            await firebase.setData(mainUsersPath, socketUser.Id.ToString(), userInfo);
                            return;
                        }
                        else if (socketVoiceState.VoiceChannel.Name == "AFK♿")
                        {
                            userInfo.joinedVoice = DateTime.UtcNow;
                            await firebase.setData(mainUsersPath, socketUser.Id.ToString(), userInfo);
                            return;
                        }
                    }
                }
                if (socketVoiceState.VoiceChannel != null)
                {
                    if (socketVoiceState.VoiceChannel.Name == "AFK♿")
                    {
                        userInfo.joinedVoice = DateTime.UtcNow;
                        await firebase.setData(mainUsersPath, socketUser.Id.ToString(), userInfo);
                        return;
                    }
                }
            }
            else if (socketVoiceState.VoiceChannel.Name == "AFK♿")
            {
                userInfo.joinedVoice = DateTime.UtcNow;
                await firebase.setData(mainUsersPath, socketUser.Id.ToString(), userInfo);
                return;
            }

            if ((userVoiceStateUpdated.IsDeafened || userVoiceStateUpdated.IsSelfDeafened) && userVoiceStateUpdated.VoiceChannel != null)
            {
                if (socketVoiceState.IsDeafened && userVoiceStateUpdated.IsDeafened || socketVoiceState.IsSelfDeafened && userVoiceStateUpdated.IsSelfDeafened)
                    return;

                if ((socketVoiceState.IsSelfDeafened || userVoiceStateUpdated.IsSelfDeafened) && socketVoiceState.VoiceChannel == null)
                    return;

                TimeSpan result = DateTime.UtcNow.Subtract(userInfo.joinedVoice);
                int seconds = Convert.ToInt32(result.TotalSeconds);
                userInfo.joinedVoice = DateTime.UtcNow;
                userInfo.coinsCount = userInfo.coinsCount + (seconds * userInfo.userRate);
                await firebase.setData(mainUsersPath, socketUser.Id.ToString(), userInfo);
            }

            if (socketVoiceState.VoiceChannel != null && (socketVoiceState.IsMuted != userVoiceStateUpdated.IsMuted && (socketVoiceState.IsDeafened == userVoiceStateUpdated.IsDeafened) 
                || socketVoiceState.IsSelfMuted != userVoiceStateUpdated.IsSelfMuted && (socketVoiceState.IsSelfDeafened == userVoiceStateUpdated.IsSelfDeafened)) 
                || socketVoiceState.VoiceChannel != userVoiceStateUpdated.VoiceChannel && socketVoiceState.VoiceChannel != null && userVoiceStateUpdated.VoiceChannel != null)
                return;

            if (guildUser.VoiceChannel != null && !guildUser.IsSelfDeafened && !guildUser.IsDeafened)
            {
                userInfo.joinedVoice = DateTime.UtcNow;
                await firebase.setData(mainUsersPath, socketUser.Id.ToString(), userInfo);
                return;
            }
            else if(!guildUser.IsSelfDeafened && !guildUser.IsDeafened && !userVoiceStateUpdated.IsDeafened && !userVoiceStateUpdated.IsSelfDeafened)
            {
                TimeSpan result = DateTime.UtcNow.Subtract(userInfo.joinedVoice);
                int seconds = Convert.ToInt32(result.TotalSeconds);
                userInfo.joinedVoice = DateTime.UtcNow;
                userInfo.coinsCount = userInfo.coinsCount + (seconds * userInfo.userRate);
                await firebase.setData(mainUsersPath, socketUser.Id.ToString(), userInfo);
            }
        }

        private async Task MessageReceived(SocketMessage message)
        {
            SocketUser user = message.Author;
            var guildUser = user as SocketGuildUser;

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
                if (roles.Contains(role.ToLower()))
                {
                    
                    var userRole = (guildUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());

                    if (guildUser.Roles.Contains(userRole))
                    {
                        await guildUser.RemoveRoleAsync(userRole);
                        await message.Channel.SendMessageAsync($"Yay, you're leaved club {role}, dude");
                    }
                    else
                    {
                        await guildUser.AddRoleAsync(userRole);
                        await message.Channel.SendMessageAsync($"Yay, you're now in club {role}, dude");
                    }

                }
            }

            if (tokens[0] == "!removerole")
            {
                var userRole = (guildUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());
                if (guildUser.Roles.Contains(userRole))
                {
                    await guildUser.RemoveRoleAsync(userRole);
                    await message.Channel.SendMessageAsync($"Yay, you're leaved club {role}, dude");
                } else await message.Channel.SendMessageAsync("This role doesn't exist <:FeelsBadMan:456407012243275787>");

            }

            if (user.Username == meeBot)
            {
                if (message.Content.Equals("") && message.Attachments.Count == 1)
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

            //logs to chat
            await writeToLogChannel(message, null, "recieved");


            if (message.Content.ToString()[0] == '!')
            {
                //Create new thread
                //System.Threading.Thread.Sleep(10000);
                //await message.DeleteAsync();
            }

            await addCoinAsync(user.Id);
        }

        public async Task writeToLogChannel(SocketMessage message, SocketMessage after, string eventName)
        {
            //logs to chat
            if (!message.Channel.Name.Equals("logs") && !message.Channel.Name.Equals("nsfw_logs"))
            {
                var guildUser = message.Author as SocketGuildUser;
                ISocketMessageChannel socketMessageChannel = message.Channel;
                IGuild guild = (guildUser as IGuildUser).Guild;
                var guildChannels = guild.GetTextChannelsAsync();

                string chatLogs = "";
                if (socketMessageChannel.IsNsfw)
                    chatLogs = "nsfw_logs";
                else chatLogs = "logs";


                string attachment = "";
                foreach (var a in message.Attachments)
                {
                    attachment = a.Url;
                }

                foreach (var channel in guildChannels.Result)
                {
                    if (channel.Name.Equals(chatLogs))
                    {
                        if (eventName.Equals("recieved"))
                            await channel.SendMessageAsync($"```{message.Timestamp.DateTime} [{message.Channel}] {message.Author}: {message}``` {attachment}");
                        else if (after != null)
                        {
                            await channel.SendMessageAsync($"```{message.Timestamp.DateTime} [{message.Channel}] {message.Author}: {message}``` {attachment} eddited to ```{after}```");
                        }
                        else if (eventName.Equals("deleted"))
                        {
                            await channel.SendMessageAsync($"deleted```{message.Timestamp.DateTime} [{message.Channel}] {message.Author}: {message}``` {attachment}");
                        }
                    }
                }
            }
        }

        public async Task addCoinAsync(ulong userId)
        {
            var result = await firebase.getSingleUser("Coins", userId);
            if (result == null)
            {
                Coins user = new Coins()
                {
                    coinsCount = 1,
                    userId = userId,
                };

                await firebase.setData("Coins", userId.ToString(), user);
            }
            else
            {

                var coins = result.coinsCount + 1;
                Coins data = new Coins()
                {
                    coinsCount = coins,
                    userId = userId,
                };

                await firebase.updateData("Coins", userId.ToString(), data);
            }
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message.Timestamp.DateTime} [{message.Channel}] {message.Author} {message} -> {after} (eddited)");
            await writeToLogChannel(message as SocketMessage, after, "updated");
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> before, ISocketMessageChannel channel)
        {
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message.Timestamp.DateTime} [{message.Channel}] {message.Author} {message} (deleted)");
            await writeToLogChannel(message as SocketMessage, null, "deleted");
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }
}