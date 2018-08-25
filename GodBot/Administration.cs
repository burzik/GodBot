using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GodBot
{
    class Administration
    {
        public static async Task messageLogger(SocketMessage message, SocketMessage after, string eventName)
        {
            //TODO soft & strict logs
            //TODO Markdown messages 
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
                    attachment = a.Url;

                foreach (var channel in guildChannels.Result)
                {
                    if (channel.Name.Equals(chatLogs))
                    {
						if (eventName.Equals("recieved"))
						{
							await channel.SendMessageAsync($"```{message.Timestamp.DateTime} [{message.Channel}] {message.Author}: {message}``` {attachment}");
							Console.WriteLine($"{message.Timestamp.DateTime} [{message.Channel}] {message.Author}: {message} {attachment}");
						}
						else if (after != null)
						{
							await channel.SendMessageAsync($"```{message.Timestamp.DateTime} [{message.Channel}] {message.Author}: {message}``` {attachment} eddited to ```{after}```");
							Console.WriteLine($"{message.Timestamp.DateTime} [{message.Channel}] {message.Author} {message} -> {after} (eddited)");
						}
						else if (eventName.Equals("deleted"))
						{
							await channel.SendMessageAsync($"deleted```{message.Timestamp.DateTime} [{message.Channel}] {message.Author}: {message}``` {attachment}");
							Console.WriteLine($"{message.Timestamp.DateTime} [{message.Channel}] {message.Author} {message} (deleted)");
						}
					}
                }
            }
        }

        public static async Task addRole(SocketMessage message, string[] tokens, HashSet<String> roles, string role)
        {
            var guildUser = message.Author as SocketGuildUser;

            if (tokens[0] == "!addrole")
            {
                if (roles.Contains(role.ToLower()))
                {
                    var userRole = (guildUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());

                    if (guildUser.Roles.Contains(userRole))
                    {
                        await message.Channel.SendMessageAsync($"Yay, you're already in club {role}, dude\n Use !removerole <roleName> to leave club");
                    }
                    else
                    {
                        await guildUser.AddRoleAsync(userRole);
                        await message.Channel.SendMessageAsync($"Yay, you're now in club {role}, dude");
                    }
                }
				else await message.Channel.SendMessageAsync("This role doesn't exist <:FeelsBadMan:456407012243275787>");
			}
        }

        public static async Task removeRole(SocketMessage message, string[] tokens, string role)
        {
            var guildUser = message.Author as SocketGuildUser;

            if (tokens[0] == "!removerole")
            {
                var userRole = (guildUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());
                if (guildUser.Roles.Contains(userRole))
                {
                    await guildUser.RemoveRoleAsync(userRole);
                    await message.Channel.SendMessageAsync($"Yay, you're leaved club {role}, dude");
                }
                else await message.Channel.SendMessageAsync("This role doesn't exist <:FeelsBadMan:456407012243275787>");
            }
        }

		public static async Task deleteMessage()
		{
			//TODO Create new thread
			/*
			if (message.Content.ToString()[0] == '!')
			{
				System.Threading.Thread.Sleep(10000);
				await message.DeleteAsync();
			}
			*/
		}

	}
}
