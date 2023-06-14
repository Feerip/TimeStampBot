using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _04_interactions_framework.Modules
{
    // Interation modules must be public and inherit from an IInterationModuleBase
    public class UtilityModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private CommandHandler _handler;

        // Constructor injection is also a valid way to access the dependecies
        public UtilityModule(CommandHandler handler)
        {
            _handler = handler;
        }

        private async Task BozoLog(string functionName)
        {
            try
            {
                Console.WriteLine($"Executing {functionName} for {Context.User.Username}#{Context.User.Discriminator} () " +
                $"in #{Context.Channel.Name} " + $"of {Context.Guild.Name}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private async Task BozoDMLog(string functionName)
        {
            Console.WriteLine($"Executing {functionName} for {Context.User.Username}#{Context.User.Discriminator} () " +
                $"in their DMs");
        }

        [SlashCommand("timestamp", "Generate a timestamp")]
        public async Task Timestamp(
            [Discord.Interactions.Summary(description: "I'm smart, I can understand lots of formats. Examples: \"5:30 pm\" or \"12/24/2022 23:59\"")] string dateTime,
            TimestampFormat? format = null
            )
        {
            await BozoLog("Timestamp()");
            Database db = new();
            if (db.GetDiscordUserTimeZone(Context.User.Id) is null)
            {
                await SetNewTimeZone();
                return;
            }

            //// Check for AoD timestamp
            //if (dateTime.Equals("aod", StringComparison.OrdinalIgnoreCase))
            //{
            //    // Get current date and time
            //    DateTime timeNow = DateTime.Now;
            //    // Set the initial target date to today (as per excution date) and its time to our normal AoD time (6pm) with offset PST to account for server location.
            //    // This should theoretically take care of daylight savings time requirement, changing it between 1am and 2am UTC based on the server's local time zone info.
            //    DateTimeOffset aod_dto = new(timeNow.Year, timeNow.Month, timeNow.Day, 18, 0, 0, TimeZoneInfo.FindSystemTimeZoneById("US/Pacific").BaseUtcOffset);

            //    if (timeNow.Hour > 18)  // If current time is after 6pm, we need to add a day because this is for tomorrow.
            //    {
            //        aod_dto = aod_dto.AddDays(1);
            //    }
            //    // Otherwise, we're already done.
                
            //    // Give the user exactly what they want
            //    await RespondAsync($"Your <@&792538753762590790> timestamp:\n<t:{aod_dto.ToUnixTimeSeconds()}:f>```<t:{aod_dto.ToUnixTimeSeconds()}:f>```", ephemeral: true);
            //    // And then fuck off
            //    return;
            //}

            // Regular timestamp
            DateTime dateValue;

            DateTime.TryParse(dateTime, out dateValue);

            try
            {
                dateValue = dateValue.AddTicks(TimeZoneInfo.FindSystemTimeZoneById("US/Pacific").BaseUtcOffset.Ticks);
                dateValue = dateValue.AddTicks(-TimeZoneInfo.FindSystemTimeZoneById(db.GetDiscordUserTimeZone(Context.User.Id)).BaseUtcOffset.Ticks);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await RespondAsync($"Failed to parse timestamp: ```{ex.ToString()}```");
                return;
            }
            DateTimeOffset dto = new(dateValue);

            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(db.GetDiscordUserTimeZone(Context.User.Id));

            //await RespondAsync($"Your current time zone setting: **{userTimeZone.Id} {userTimeZone.DisplayName.Substring(0, 11)}**\n" +
            //    $"Just to double check, the time you're talking about was/is <t:{dto.ToUnixTimeSeconds()}:R>.\n" +
            //    $"Your final timestamp will look like this: <t:{dto.ToUnixTimeSeconds()}:{formatCode}>\n" +
            //    $"If this is not correct, please dismiss this message and try again.\n" +
            //    $"Otherwise, here's your timestamp:\n" +
            //    $"\\<t:{dto.ToUnixTimeSeconds()}:{formatCode}>", ephemeral: true);

            string replyString = "";
            if (format is not null)
            {
                var result = -(dateValue - DateTime.Now);

                switch (format)
                {
                    case TimestampFormat.SecondsPassed:
                        replyString = $"`{result.TotalSeconds} seconds` have passed since <t:{dto.ToUnixTimeSeconds()}:F>";
                        break;
                    case TimestampFormat.MinutesPassed:
                        replyString = $"`{result.TotalMinutes} minutes` have passed since <t:{dto.ToUnixTimeSeconds()}:F>";
                        break;
                    case TimestampFormat.HoursPassed:
                        replyString = $"`{result.TotalHours} hours` have passed since <t:{dto.ToUnixTimeSeconds()}:F>";
                        break;
                    case TimestampFormat.DaysPassed:
                        replyString = $"`{result.TotalDays} days` have passed since <t:{dto.ToUnixTimeSeconds()}:F>";
                        break;
                    case TimestampFormat.WeeksPassed:
                        replyString = $"`{result.TotalDays/7d} weeks` have passed since <t:{dto.ToUnixTimeSeconds()}:F>";
                        break;
                    case TimestampFormat.MonthsPassed:
                        replyString = $"`{result.TotalDays/30d} months` have passed since <t:{dto.ToUnixTimeSeconds()}:F>";
                        break;
                    case TimestampFormat.YearsPassed:
                        replyString = $"`{result.TotalDays/365d} years` have passed since <t:{dto.ToUnixTimeSeconds()}:F>";
                        break;
                }
                await RespondAsync(replyString, ephemeral: false);
                return;
            }

            replyString =
                $"Just to double check, the time you're talking about is/was <t:{dto.ToUnixTimeSeconds()}:R>.\n" +
                $"If this is not correct, please dismiss this message and try again.\n" +
                $"Otherwise, here are your timestamps:\n\n" +

                $"**PROTIP:** Users can mouseover any timestamp to show the full-length format.\n" +
                $"`<t:{dto.ToUnixTimeSeconds()}:t>` => <t:{dto.ToUnixTimeSeconds()}:t>\n" +
                $"`<t:{dto.ToUnixTimeSeconds()}:T>` => <t:{dto.ToUnixTimeSeconds()}:T>\n" +
                $"`<t:{dto.ToUnixTimeSeconds()}:d>` => <t:{dto.ToUnixTimeSeconds()}:d>\n" +
                $"`<t:{dto.ToUnixTimeSeconds()}:D>` => <t:{dto.ToUnixTimeSeconds()}:D>\n" +
                $"`<t:{dto.ToUnixTimeSeconds()}:f>` => <t:{dto.ToUnixTimeSeconds()}:f>\n" +
                $"`<t:{dto.ToUnixTimeSeconds()}:F>` => <t:{dto.ToUnixTimeSeconds()}:F>\n" +
                $"`<t:{dto.ToUnixTimeSeconds()}:R>` => <t:{dto.ToUnixTimeSeconds()}:R>\n";
                
            replyString += $"\n**HINT:** If this is your first time using this feature and you don't know what to choose, " +
                $"I suggest a combination of these two formats:\n" +
                $"`<t:{dto.ToUnixTimeSeconds()}:R> at <t:{dto.ToUnixTimeSeconds()}:f>`\n" +
                $"Which you can use like this:\n" +
                $"\"My event is/was <t:{dto.ToUnixTimeSeconds()}:R> at <t:{dto.ToUnixTimeSeconds()}:f>\"";

            var builder = new EmbedBuilder()
                .WithDescription(replyString)
                .WithColor(Color.Green)
                .WithTitle($"Your Time Zone: **{userTimeZone.Id} {userTimeZone.DisplayName.Substring(0, 11)}**")
                .WithFooter("With your permission, this app only stores your DiscordID & time zone preference. Nothing else is kept. Remove your info any time with /set-new-time-zone.")
                ;
            //builder.AddField("Stamp", $"<t:{dto.ToUnixTimeSeconds()}:R>");

            await RespondAsync(embed: builder.Build(), ephemeral: true);

        }

        [SlashCommand("set-new-time-zone", "Set your local timezone so I know how to parse your input strings")]
        public async Task SetNewTimeZone()
        {
            await BozoLog("SetNewTimeZone()");
            Database db = new();
            string? msg = null;
            if (db.GetDiscordUserTimeZone(Context.User.Id) is null)
                msg = "Hi there! You're receiving this message because you tried to use me without having a time zone set. I can't function properly without knowing how you want me to parse your timestamps.\n\n" +
                    "Please choose from the following options to be able to use me: ";
            else
                msg = "Hi there! You're receiving this message because you asked me to change your time zone preference!\n\n" +
                    "Please choose from the following options: ";

            TimeZoneSelectMenuBuilder tzMenuBuilder = new();


            var backButtonBuilder = new ButtonBuilder()
                .WithCustomId("time-zone-menu:0")
                .WithEmote(new Emoji("⬅️"))
                .WithStyle(ButtonStyle.Primary)
                .WithDisabled(true)
                ;
            var forwardButtonBuilder = new ButtonBuilder()
                .WithCustomId("time-zone-menu:1")
                .WithEmote(new Emoji("➡️"))
                .WithStyle(ButtonStyle.Primary)
                .WithDisabled(false)
                ;
            var removeMyInfoButtonBuilder = new ButtonBuilder()
                .WithCustomId($"remove-my-info")
                .WithLabel("Remove my info completely")
                .WithStyle(ButtonStyle.Danger)
                .WithDisabled(db.GetDiscordUserTimeZone(Context.User.Id) is null ? true : false)
                ;




            var builder = new ComponentBuilder()
                .WithSelectMenu(tzMenuBuilder.MenuBuilder(0), row: 0)
                .WithButton(backButtonBuilder, row: 1)
                .WithButton(forwardButtonBuilder, row: 1)
                .WithButton(removeMyInfoButtonBuilder, row: 1)
                ;

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Time Zone Setting")
                .WithDescription(msg)
                .WithColor(db.GetDiscordUserTimeZone(Context.User.Id) is null ? Color.Red : Color.Green)
                .WithThumbnailUrl("https://cdn.discordapp.com/emojis/774545768564391947.webp")
                ;
            await DeferAsync(ephemeral: true);
            await foreach (var messages in (await Context.User.CreateDMChannelAsync()).GetMessagesAsync(100))
            {
                foreach (var message in messages)
                {
                    if (message.Author.Id == Context.Client.CurrentUser.Id)
                        await Context.User.CreateDMChannelAsync().Result.DeleteMessageAsync(message.Id);
                }
            }
            //await Context.User.CreateDMChannelAsync().Result.
            await Context.User.SendMessageAsync(embed: embedBuilder.Build(), components: builder.Build());
            await FollowupAsync("I sent you a DM :)", ephemeral: true);

        }
        [ComponentInteraction("time-zone-menu:*")]
        public async Task TimeZoneMenuNavigate(string currentPage)
        {
            await BozoDMLog($"TimeZoneMenuNavigate({currentPage})");
            int intCurrentPage = int.Parse(currentPage);
            int previousPage = intCurrentPage - 1;
            int nextPage = intCurrentPage + 1;
            // ...
            await (Context.Interaction as SocketMessageComponent).ModifyOriginalResponseAsync(msg =>
            {
                Context.Interaction.DeferAsync();
                TimeZoneSelectMenuBuilder tzMenuBuilder = new();

                Database db = new();


                var backButtonBuilder = new ButtonBuilder()
                    .WithCustomId($"time-zone-menu:{previousPage}")
                    .WithEmote(new Emoji("⬅️"))
                    //.WithLabel("Button")
                    .WithStyle(ButtonStyle.Primary)
                    .WithDisabled(previousPage >= 0 ? false : true)
                    ;
                var forwardButtonBuilder = new ButtonBuilder()
                    .WithCustomId($"time-zone-menu:{nextPage}")
                    .WithEmote(new Emoji("➡️"))
                    .WithStyle(ButtonStyle.Primary)
                    .WithDisabled(nextPage < tzMenuBuilder.numPages ? false : true)
                    ;
                var removeMyInfoButtonBuilder = new ButtonBuilder()
                    .WithCustomId($"remove-my-info")
                    .WithLabel("Remove my info from the app")
                    .WithStyle(ButtonStyle.Danger)
                    .WithDisabled(db.GetDiscordUserTimeZone(Context.User.Id) is null ? true : false)
                    ;


                var builder = new ComponentBuilder()
                .WithSelectMenu(tzMenuBuilder.MenuBuilder(intCurrentPage), row: 0)
                .WithButton(backButtonBuilder, row: 1)
                .WithButton(forwardButtonBuilder, row: 1)
                .WithButton(removeMyInfoButtonBuilder, row: 1)
                ;

                msg.Components = builder.Build();

            });
        }


        [ComponentInteraction("remove-my-info")]
        public async Task RemoveMyInfo()
        {
            await BozoDMLog("RemoveMyInfo()");
            await DeferAsync(ephemeral: true);
            Database db = new();
            db.RemoveRow(Context.User.Id);
            string failedMessage = "Failed to delete data, unknown reason. Please report this to my owner so he can go in and manually delete it.";
            if (db.GetDiscordUserTimeZone(Context.User.Id) is not null)
            {
                await ReplyAsync(failedMessage);
                throw new Exception(failedMessage);
            }
            else
            {
                await Context.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    string messageContent = $"Your info (DiscordID & time zone preference) has been completely removed from the app. It's as if you were never here...";
                    string footerContent = $"You're welcome back any time though!";
                    var embedBuilder = new EmbedBuilder()
                        .WithTitle("All done...")
                        .WithDescription(messageContent)
                        .WithFooter(footerContent)
                        .WithColor(Color.Red)
                        .WithThumbnailUrl("https://cdn.discordapp.com/emojis/861518094522515507.webp")
                        ;
                    msg.Embed = embedBuilder.Build();
                    msg.Components = new ComponentBuilder().Build();
                });
            }

        }
#if DEBUG
        [SlashCommand("delete-commands", "Delete all commands")]
        [RequireOwner]
        public async Task DeleteCommands()
        {
            await DeferAsync(ephemeral: true);
            await Context.Guild.DeleteApplicationCommandsAsync();
            await Context.Interaction.FollowupAsync("Guild commands deleted!", ephemeral: true);
        }
#endif

        //// Slash Commands are declared using the [SlashCommand], you need to provide a name and a description, both following the Discord guidelines
        //[SlashCommand("ping", "Recieve a pong")]
        //// By setting the DefaultPermission to false, you can disable the command by default. No one can use the command until you give them permission
        //[DefaultPermission(false)]
        //public async Task Ping()
        //{
        //    await RespondAsync("pong");
        //}

        // You can use a number of parameter types in you Slash Command handlers (string, int, double, bool, IUser, IChannel, IMentionable, IRole, Enums) by default. Optionally,
        // you can implement your own TypeConverters to support a wider range of parameter types. For more information, refer to the library documentation.
        // Optional method parameters(parameters with a default value) also will be displayed as optional on Discord.

        //// [Summary] lets you customize the name and the description of a parameter
        //[SlashCommand("echo", "Repeat the input")]
        //public async Task Echo(string echo, [Summary(description: "mention the user")] bool mention = false)
        //{
        //    await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));
        //}

        //// [Group] will create a command group. [SlashCommand]s and [ComponentInteraction]s will be registered with the group prefix
        //[Group("test_group", "This is a command group")]
        //public class GroupExample : InteractionModuleBase<SocketInteractionContext>
        //{
        //    // You can create command choices either by using the [Choice] attribute or by creating an enum. Every enum with 25 or less values will be registered as a multiple
        //    // choice option
        //    [SlashCommand("choice_example", "Enums create choices")]
        //    public async Task ChoiceExample(ExampleEnum input)
        //    {
        //        await RespondAsync(input.ToString());
        //    }
        //}

        //// User Commands can only have one parameter, which must be a type of SocketUser
        //[UserCommand("SayHello")]
        //public async Task SayHello(IUser user)
        //{
        //    await RespondAsync($"Hello, {user.Mention}");
        //}

        //// Message Commands can only have one parameter, which must be a type of SocketMessage
        //[MessageCommand("Delete")]
        //[RequireOwner]
        //public async Task DeleteMesage(IMessage message)
        //{
        //    await message.DeleteAsync();
        //    await RespondAsync("Deleted message.");
        //}

        // Use [ComponentInteraction] to handle message component interactions. Message component interaction with the matching customId will be executed.
        // Alternatively, you can create a wild card pattern using the '*' character. Interaction Service will perform a lazy regex search and capture the matching strings.
        //// You can then access these capture groups from the method parameters, in the order they were captured. Using the wild card pattern, you can cherry pick component interactions.
        //[ComponentInteraction("musicSelect:*,*")]
        //public async Task ButtonPress(string id, string name)
        //{
        //    // ...
        //    await RespondAsync($"Playing song: {name}/{id}");
        //}

        //// Select Menu interactions, contain ids of the menu options that were selected by the user. You can access the option ids from the method parameters.
        //// You can also use the wild card pattern with Select Menus, in that case, the wild card captures will be passed on to the method first, followed by the option ids.
        //[ComponentInteraction("roleSelect")]
        //public async Task RoleSelect(params string[] selections)
        //{
        //    // implement
        //}
    }
}
