using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Enums;
using Discord_Bot.Processors.EmbedProcessors.Help;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using NSubstitute.Core;
using CommandResult = Discord.Commands.IResult;
using InteractionResult = Discord.Interactions.IResult;

namespace Discord_Bot.Tests.Integration.Help;

//!help and its category menu over the bot's real command modules. HelpCommands lists a module by its [Remarks] level,
//and Discord.Net refuses an embed with a missing or empty field value, more than 25 fields or 6000 characters
public sealed class HelpTests : IDisposable
{
    private const ulong Owner = DiscordIds.ThirdUser;

    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser member = DiscordMocks.GuildUser();
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public HelpTests()
    {
        host = new TestHost().Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Fact]
    public async Task EveryModule_HasAHelpLevelAndSummaries()
    {
        List<string> problems = [];
        foreach (ModuleInfo module in await ModulesAsync())
        {
            if (!Enum.TryParse(module.Remarks, out CommandLevelEnum level) || !Enum.IsDefined(level))
            {
                problems.Add($"{module.Name}: [Remarks(\"{module.Remarks}\")] is not a CommandLevelEnum name, so !help never lists it");
            }
            if (string.IsNullOrWhiteSpace(module.Summary))
            {
                problems.Add($"{module.Name}: no [Summary]");
            }
            problems.AddRange(module.Commands.Where(command => string.IsNullOrWhiteSpace(command.Summary)).Select(command => $"{module.Name}: !{command.Name} has no [Summary]"));
        }

        Assert.Empty(problems);
    }

    [Fact]
    public async Task EveryLevel_HelpPageAndCategoryPages_AreValidEmbeds()
    {
        List<ModuleInfo> modules = await ModulesAsync();
        List<string> problems = [];
        foreach (CommandLevelEnum level in Enum.GetValues<CommandLevelEnum>())
        {
            List<ModuleInfo> ofLevel = modules.Where(module => module.Remarks == level.ToString()).ToList();
            Assert.NotEmpty(ofLevel);
            problems.AddRange(Try($"{level} help", () =>
            {
                Embed embed = Assert.Single(HelpEmbedProcessor.CreateEmbed(level, ofLevel, ""));
                _ = HelpEmbedProcessor.CreateComponent(level, ofLevel);
                Assert.True(embed.Length <= EmbedBuilder.MaxEmbedLength);
            }));
            foreach (ModuleInfo module in ofLevel)
            {
                problems.AddRange(Try($"{level} {module.Name}", () =>
                {
                    Embed embed = Assert.Single(HelpDetailEmbedProcessor.CreateEmbed(level, module.Name, module.Commands, ""));
                    Assert.True(embed.Length <= EmbedBuilder.MaxEmbedLength);
                }));
            }
        }

        Assert.Empty(problems);
    }

    [Fact]
    public async Task Help_ListsTheUserCategoriesWithAMenuOfThem()
    {
        List<string> userModules = (await ModulesAsync()).Where(module => module.Remarks == "User").Select(module => module.Name).ToList();

        CommandResult result = await RunAsync("!help", member);

        Assert.True(result.IsSuccess, result.ErrorReason);
        SentMessage sent = Assert.Single(channel.Sent);
        Embed embed = Assert.Single(sent.Embeds);
        Assert.Equal("User Commands", embed.Title);
        Assert.Equal(["!help", .. userModules], embed.Fields.Select(field => field.Name));
        SelectMenuComponent menu = Assert.IsType<SelectMenuComponent>(Assert.Single(Assert.IsType<ActionRowComponent>(Assert.Single(sent.Components!.Components)).Components));
        Assert.Equal("HelpMenu_User", menu.CustomId);
        Assert.Equal(userModules, menu.Options.Select(option => option.Value));
    }

    [Fact]
    public async Task Help_AdminLevelAskedByAMember_SaysNothing()
    {
        await RunAsync("!help admin", member);

        Assert.Empty(channel.Sent);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("adm")]
    [InlineData("Admin")]
    public async Task Help_AdminLevelAskedByAnAdministrator_ListsTheAdminCategories(string level)
    {
        IGuildUser admin = DiscordMocks.GuildUser(DiscordIds.OtherUser);
        admin.GuildPermissions.Returns(new GuildPermissions(administrator: true));

        await RunAsync($"!help {level}", admin);

        Assert.Equal("Admin Commands", Assert.Single(Assert.Single(channel.Sent).Embeds).Title);
    }

    [Fact]
    public async Task Help_OwnerLevelAskedByTheOwner_ListsTheOwnerCategories()
    {
        IUser owner = DiscordMocks.User(Owner);
        IApplication application = Substitute.For<IApplication>();
        application.Owner.Returns(owner);
        ICommandContext context = DiscordMocks.TextCommand("!help owner", DiscordMocks.GuildUser(Owner), channel.Channel);
        context.Client.GetApplicationInfoAsync().Returns(application);

        await host.RunCommandAsync(context);

        Assert.Equal("Owner Commands", Assert.Single(Assert.Single(channel.Sent).Embeds).Title);
    }

    [Fact]
    public async Task HelpMenu_Category_SendsItsCommandsToTheUserAlone()
    {
        IComponentInteraction choice = DiscordMocks.SelectMenuChoice("HelpMenu_User", "Reminder", member);

        InteractionResult result = await host.RunInteractionAsync(new FakeInteractionContext(choice, member, channel.Channel, guild));

        Assert.True(result.IsSuccess, result.ErrorReason);
        (Embed embed, bool ephemeral) = Followup(choice);
        Assert.True(ephemeral);
        Assert.Equal("User Reminder Commands:", embed.Title);
        EmbedField remindIn = embed.Fields.Single(field => field.Name == "!reminder in [amount*]>[message**]");
        //Reminders work in direct messages too
        Assert.StartsWith("(DM) Adding a new reminder", remindIn.Value, StringComparison.Ordinal);
        Assert.EndsWith("\nAliases: remind in, remind at, reminder at", remindIn.Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HelpMenu_ServerOnlyCommand_HasNoDirectMessageMark()
    {
        IComponentInteraction choice = DiscordMocks.SelectMenuChoice("HelpMenu_User", "Birthday", member);

        await host.RunInteractionAsync(new FakeInteractionContext(choice, member, channel.Channel, guild));

        EmbedField birthdayAdd = Followup(choice).Embed.Fields.Single(field => field.Name == "!birthday add [date*]");
        Assert.StartsWith("Adding your birthday", birthdayAdd.Value, StringComparison.Ordinal);
    }

    private async Task<List<ModuleInfo>> ModulesAsync()
    {
        //!help itself has no level; its page adds it by hand
        return (await host.GetCommandServiceAsync()).Modules.Where(module => !module.Commands.Any(command => command.Name == "help")).ToList();
    }

    private static List<string> Try(string page, Action build)
    {
        try
        {
            build();
            return [];
        }
        catch (Exception ex)
        {
            return [$"{page}: {ex.Message}"];
        }
    }

    private static (Embed Embed, bool Ephemeral) Followup(IComponentInteraction interaction)
    {
        ICall call = Assert.Single(interaction.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IDiscordInteraction.FollowupAsync));
        System.Reflection.ParameterInfo[] parameters = call.GetMethodInfo().GetParameters();
        object?[] arguments = call.GetArguments();
        bool ephemeral = (bool) arguments[Array.FindIndex(parameters, parameter => parameter.Name == "ephemeral")]!;
        return (Assert.Single(arguments.OfType<Embed[]>().Single()), ephemeral);
    }

    private Task<CommandResult> RunAsync(string text, IGuildUser author)
    {
        return host.RunCommandAsync(DiscordMocks.TextCommand(text, author, channel.Channel));
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
