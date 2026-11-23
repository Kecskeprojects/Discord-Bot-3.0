using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using CommandResult = Discord.Commands.IResult;

namespace Discord_Bot.Tests.Integration.Biases;

//The owner's !biaslist add/remove, !bias alias add/remove and !edit biasdata, through Discord.Net's command service
public sealed class OwnerBiasCommandTests : IDisposable
{
    private const ulong OwnerId = DiscordIds.ThirdUser;

    private readonly IIdolService idolService = Substitute.For<IIdolService>();
    private readonly IIdolAliasService idolAliasService = Substitute.For<IIdolAliasService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser owner = DiscordMocks.GuildUser(OwnerId);
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public OwnerBiasCommandTests()
    {
        host = new TestHost().Replace(idolService).Replace(idolAliasService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Bias added to list.")]
    [InlineData(DbProcessResultEnum.AlreadyExists, "Bias already in database.")]
    [InlineData(DbProcessResultEnum.Failure, "Bias could not be added!")]
    public async Task BiaslistAdd_NameAndGroup_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        idolService.AddIdolAsync("jisoo", "blackpink").Returns(outcome);

        CommandResult result = await RunAsync("!biaslist add Jisoo-BLACKPINK", owner);

        Assert.True(result.IsSuccess, result.ErrorReason);
        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Bias removed from list.")]
    [InlineData(DbProcessResultEnum.NotFound, "Bias could not be found.")]
    public async Task BiaslistRemove_NameAndGroup_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        idolService.RemoveIdolAsync("jisoo", "blackpink").Returns(outcome);

        await RunAsync("!biaslist remove jisoo-blackpink", owner);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Bias alias added to list.")]
    [InlineData(DbProcessResultEnum.AlreadyExists, "Bias alias already in database.")]
    [InlineData(DbProcessResultEnum.NotFound, "Bias with that name not found in database.")]
    public async Task BiasAliasAdd_AliasNameAndGroup_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        idolAliasService.AddIdolAliasAsync("chu", "jisoo", "blackpink").Returns(outcome);

        await RunAsync("!bias alias add chu-jisoo-blackpink", owner);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Bias alias removed from list.")]
    [InlineData(DbProcessResultEnum.NotFound, "Bias alias not in database.")]
    public async Task BiasAliasRemove_AliasNameAndGroup_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        idolAliasService.RemoveIdolAliasAsync("chu", "jisoo", "blackpink").Returns(outcome);

        await RunAsync("!bias alias remove chu-jisoo-blackpink", owner);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
    }

    [Fact]
    public async Task EditBiasData_NameAndGroup_OffersTheEditMenu()
    {
        await RunAsync("!edit biasdata jisoo-blackpink", owner);

        SentMessage sent = Assert.Single(channel.Sent);
        Assert.Equal("What action would you like to perform", sent.Text);
        SelectMenuComponent menu = Assert.IsType<SelectMenuComponent>(Assert.Single(Assert.IsType<ActionRowComponent>(Assert.Single(sent.Components!.Components)).Components));
        Assert.Equal("EditIdolData", menu.CustomId);
    }

    //They answered nothing until 2026-11-23
    [Theory]
    [InlineData("!biaslist add jisoo", "Give both the name and the group in the following format: [name]-[group].")]
    [InlineData("!biaslist remove jisoo", "Give both the name and the group in the following format: [name]-[group].")]
    [InlineData("!edit biasdata jisoo", "Give both the name and the group in the following format: [name]-[group].")]
    [InlineData("!bias alias add chu-jisoo", "Give the alias, the name and the group in the following format: [alias]-[name]-[group].")]
    [InlineData("!bias alias remove chu-jisoo", "Give the alias, the name and the group in the following format: [alias]-[name]-[group].")]
    public async Task OwnerCommands_PartMissing_SayWhatToGive(string text, string reply)
    {
        await RunAsync(text, owner);

        Assert.Equal(reply, Assert.Single(channel.Sent).Text);
        await idolService.DidNotReceiveWithAnyArgs().AddIdolAsync(default!, default!);
        await idolAliasService.DidNotReceiveWithAnyArgs().AddIdolAliasAsync(default!, default!, default!);
    }

    [Fact]
    public async Task BiaslistAdd_SomeoneElse_IsRefusedByTheCommandService()
    {
        CommandResult result = await RunAsync("!biaslist add jisoo-blackpink", DiscordMocks.GuildUser());

        Assert.Equal(CommandError.UnmetPrecondition, result.Error);
        await idolService.DidNotReceiveWithAnyArgs().AddIdolAsync(default!, default!);
    }

    private Task<CommandResult> RunAsync(string text, IGuildUser author)
    {
        IUser applicationOwner = DiscordMocks.User(OwnerId);
        IApplication application = Substitute.For<IApplication>();
        application.Owner.Returns(applicationOwner);
        ICommandContext context = DiscordMocks.TextCommand(text, author, channel.Channel);
        context.Client.TokenType.Returns(TokenType.Bot);
        context.Client.GetApplicationInfoAsync().Returns(application);
        return host.RunCommandAsync(context);
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
