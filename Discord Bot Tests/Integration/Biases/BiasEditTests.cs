using DatabaseORM.Communication.Bias;
using DatabaseORM.Enums;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Interactions;
using Discord_Bot.Enums;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tests.Mocks.Discord;
using NSubstitute.Core;
using InteractionResult = Discord.Interactions.IResult;

namespace Discord_Bot.Tests.Integration.Biases;

//!edit biasdata's menu and the modals it opens, through Discord.Net's interaction service. The owner picks an action,
//the bot opens a modal filled with the idol's data, and the submitted modal saves it
public sealed class BiasEditTests : IDisposable
{
    private const ulong OwnerId = DiscordIds.ThirdUser;
    private const int IdolId = 5;
    private const int GroupId = 3;

    private readonly IIdolService idolService = Substitute.For<IIdolService>();
    private readonly IIdolGroupService idolGroupService = Substitute.For<IIdolGroupService>();
    private readonly IIdolImageService idolImageService = Substitute.For<IIdolImageService>();
    private readonly IServerService serverService = Substitute.For<IServerService>();
    private readonly TestHost host;

    private readonly IGuild guild = DiscordMocks.Guild();
    private readonly IGuildUser owner = DiscordMocks.GuildUser(OwnerId);
    private readonly RecordingChannel channel = RecordingChannel.Text();

    public BiasEditTests()
    {
        host = new TestHost().Replace(idolService).Replace(idolGroupService).Replace(idolImageService).Replace(serverService);
        serverService.GetByDiscordIdAsync(DiscordIds.Server).Returns(new ServerResource { DiscordId = DiscordIds.Server });
        //Only the name, group and ID are set, as for an idol the scraper hasn't filled in yet
        idolService.GetIdolDetailsAsync("jisoo", "blackpink").Returns(new IdolExtendedResource { IdolId = IdolId, Name = "jisoo", GroupName = "blackpink" });
        idolGroupService.GetIdolGroupDetailsAsync("blackpink").Returns(new IdolGroupExtendedResource { GroupId = GroupId, Name = "blackpink" });
        ((ITextChannel) channel.Channel).Guild.Returns(guild);
    }

    [Theory]
    [InlineData(BiasEditActionTypeEnum.EditIdol, "EditIdolModal_5")]
    [InlineData(BiasEditActionTypeEnum.EditIdolExtended, "EditIdolExtendedModal_5")]
    [InlineData(BiasEditActionTypeEnum.EditGroup, "EditGroupModal_3")]
    [InlineData(BiasEditActionTypeEnum.ChangeProfileLink, "ChangeIdolProfileLinkModal_5")]
    [InlineData(BiasEditActionTypeEnum.ChangeGroup, "ChangeIdolGroupModal_5")]
    [InlineData(BiasEditActionTypeEnum.OverrideImage, "OverrideImageModal_5")]
    public async Task EditMenu_Action_OpensItsModalForTheIdol(BiasEditActionTypeEnum action, string modalId)
    {
        IComponentInteraction choice = DiscordMocks.SelectMenuChoice("EditIdolData", $"{(int) action};jisoo;blackpink", owner);

        InteractionResult result = await RunAsync(choice, owner);

        Assert.True(result.IsSuccess, result.ErrorReason);
        await choice.Received().RespondWithModalAsync(Arg.Is<Modal>(modal => modal.CustomId == modalId), Arg.Any<RequestOptions>());
    }

    [Fact]
    public async Task EditMenu_EditIdol_FillsTheModalWithTheIdolsData()
    {
        idolService.GetIdolDetailsAsync("jisoo", "blackpink").Returns(new IdolExtendedResource
        {
            IdolId = IdolId,
            Name = "jisoo",
            GroupName = "blackpink",
            DateOfBirth = new DateOnly(1995, 1, 3),
            Gender = "F"
        });
        IComponentInteraction choice = DiscordMocks.SelectMenuChoice("EditIdolData", $"{(int) BiasEditActionTypeEnum.EditIdol};jisoo;blackpink", owner);

        await RunAsync(choice, owner);

        Modal modal = (Modal) Assert.Single(choice.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IDiscordInteraction.RespondWithModalAsync)).GetArguments()[0]!;
        Dictionary<string, string> values = modal.Component.Components.SelectMany(Inputs).ToDictionary(input => input.CustomId, input => input.Value);
        Assert.Equal("jisoo", values["name"]);
        Assert.Equal("blackpink", values["group"]);
        Assert.Equal("1995. 01. 03.", values["dateofbirth"]);
        Assert.Equal("F", values["gender"]);
    }

    [Theory]
    [InlineData(BiasEditActionTypeEnum.EditIdol, "Idol could not be found.")]
    [InlineData(BiasEditActionTypeEnum.EditGroup, "Group could not be found.")]
    public async Task EditMenu_UnknownIdol_SaysSo(BiasEditActionTypeEnum action, string reply)
    {
        IComponentInteraction choice = DiscordMocks.SelectMenuChoice("EditIdolData", $"{(int) action};nobody;nowhere", owner);

        await RunAsync(choice, owner);

        Assert.Equal(reply, Response(choice).Text);
    }

    [Theory]
    [InlineData(DbProcessResultEnum.Success, "Images removed successfully!")]
    [InlineData(DbProcessResultEnum.NotFound, "Idol could not be found.")]
    [InlineData(DbProcessResultEnum.Failure, "Images could not be removed!")]
    public async Task EditMenu_RemoveImages_GivesTheResult(DbProcessResultEnum outcome, string reply)
    {
        idolImageService.RemoveIdolImagesAsync("jisoo", "blackpink").Returns(outcome);
        IComponentInteraction choice = DiscordMocks.SelectMenuChoice("EditIdolData", $"{(int) BiasEditActionTypeEnum.RemoveImage};jisoo;blackpink", owner);

        await RunAsync(choice, owner);

        Assert.Equal(reply, Response(choice).Text);
    }

    [Fact]
    public async Task EditMenu_SomeoneElse_IsRefused()
    {
        IGuildUser member = DiscordMocks.GuildUser();
        IComponentInteraction choice = DiscordMocks.SelectMenuChoice("EditIdolData", $"{(int) BiasEditActionTypeEnum.EditIdol};jisoo;blackpink", member);

        InteractionResult result = await RunAsync(choice, member);

        Assert.Equal(InteractionCommandError.UnmetPrecondition, result.Error);
        await idolService.DidNotReceiveWithAnyArgs().GetIdolDetailsAsync(default!, default!);
    }

    [Fact]
    public async Task EditIdolModal_Submitted_SavesTheValues()
    {
        idolService.UpdateAsync(IdolId, Arg.Any<IdolEditData>()).Returns(DbProcessResultEnum.Success);
        IModalInteraction submit = DiscordMocks.ModalSubmit($"EditIdolModal_{IdolId}", new Dictionary<string, string>
        {
            ["profileurl"] = "https://example.com/jisoo",
            ["name"] = "jisoo",
            ["group"] = "blackpink",
            ["dateofbirth"] = "1995. 01. 03.",
            ["gender"] = "F"
        }, owner);

        InteractionResult result = await RunAsync(submit, owner);

        Assert.True(result.IsSuccess, result.ErrorReason);
        await idolService.Received().UpdateAsync(IdolId, Arg.Is<IdolEditData>(data =>
            data.ProfileUrl == "https://example.com/jisoo" && data.Name == "jisoo" && data.Group == "blackpink" && data.DateOfBirth == "1995. 01. 03." && data.Gender == "F"));
        (string? text, bool ephemeral) = Response(submit);
        Assert.Equal(("Edited bias successfully!", true), (text, ephemeral));
    }

    [Fact]
    public async Task EditIdolExtendedModal_Submitted_SavesTheValues()
    {
        idolService.UpdateAsync(IdolId, Arg.Any<IdolExtendedEditData>()).Returns(DbProcessResultEnum.Success);
        IModalInteraction submit = DiscordMocks.ModalSubmit($"EditIdolExtendedModal_{IdolId}", new Dictionary<string, string>
        {
            ["stagename"] = "Jisoo",
            ["fullname"] = "Kim Ji-soo",
            ["koreanstagename"] = "지수",
            ["koreanfullname"] = "김지수",
            ["debutdate"] = "2016. 08. 08."
        }, owner);

        await RunAsync(submit, owner);

        await idolService.Received().UpdateAsync(IdolId, Arg.Is<IdolExtendedEditData>(data =>
            data.StageName == "Jisoo" && data.FullName == "Kim Ji-soo" && data.KoreanStageName == "지수" && data.KoreanFullName == "김지수" && data.DebutDate == "2016. 08. 08."));
        Assert.Equal("Edited bias successfully!", Response(submit).Text);
    }

    [Fact]
    public async Task EditGroupModal_Submitted_SavesTheValues()
    {
        idolGroupService.UpdateAsync(GroupId, Arg.Any<IdolGroupEditData>()).Returns(DbProcessResultEnum.Success);
        IModalInteraction submit = DiscordMocks.ModalSubmit($"EditGroupModal_{GroupId}", new Dictionary<string, string>
        {
            ["name"] = "blackpink",
            ["fullname"] = "BLACKPINK",
            ["fullkoreanname"] = "블랙핑크",
            ["debutdate"] = "2016. 08. 08."
        }, owner);

        await RunAsync(submit, owner);

        await idolGroupService.Received().UpdateAsync(GroupId, Arg.Is<IdolGroupEditData>(data => data.Name == "blackpink" && data.FullName == "BLACKPINK"));
        Assert.Equal("Edited group successfully!", Response(submit).Text);
    }

    [Fact]
    public async Task SingleValueModals_Submitted_SaveTheirValue()
    {
        idolService.UpdateProfileUrlAsync(IdolId, "https://example.com/jisoo").Returns(DbProcessResultEnum.Success);
        idolService.ChangeGroupAsync(IdolId, "soloist").Returns(DbProcessResultEnum.Success);
        idolImageService.AddOverrideAsync(IdolId, "https://example.com/jisoo.png").Returns(DbProcessResultEnum.Success);

        IModalInteraction profile = DiscordMocks.ModalSubmit($"ChangeIdolProfileLinkModal_{IdolId}", new Dictionary<string, string> { ["profileurl"] = "https://example.com/jisoo" }, owner);
        IModalInteraction group = DiscordMocks.ModalSubmit($"ChangeIdolGroupModal_{IdolId}", new Dictionary<string, string> { ["group"] = "soloist" }, owner);
        IModalInteraction image = DiscordMocks.ModalSubmit($"OverrideImageModal_{IdolId}", new Dictionary<string, string> { ["currentimageurl"] = "", ["imageurl"] = "https://example.com/jisoo.png" }, owner);

        await RunAsync(profile, owner);
        await RunAsync(group, owner);
        await RunAsync(image, owner);

        IModalInteraction[] submits = [profile, group, image];
        Assert.All(submits, submit => Assert.Equal("Edited bias successfully!", Response(submit).Text));
    }

    [Fact]
    public async Task EditIdolModal_SaveFails_SaysSo()
    {
        idolService.UpdateAsync(IdolId, Arg.Any<IdolEditData>()).Returns(DbProcessResultEnum.Failure);
        IModalInteraction submit = DiscordMocks.ModalSubmit($"EditIdolModal_{IdolId}", new Dictionary<string, string> { ["name"] = "jisoo", ["group"] = "blackpink" }, owner);

        await RunAsync(submit, owner);

        Assert.Equal("Bias could not be edited!", Response(submit).Text);
    }

    private Task<InteractionResult> RunAsync(IDiscordInteraction interaction, IUser user)
    {
        FakeInteractionContext context = new(interaction, user, channel.Channel, guild);
        IUser applicationOwner = DiscordMocks.User(OwnerId);
        IApplication application = Substitute.For<IApplication>();
        application.Owner.Returns(applicationOwner);
        context.Client.TokenType.Returns(TokenType.Bot);
        context.Client.GetApplicationInfoAsync().Returns(application);
        return host.RunInteractionAsync(context);
    }

    //The text inputs of a modal, wherever Discord.Net put them (in rows or in labels)
    private static IEnumerable<TextInputComponent> Inputs(IMessageComponent component)
    {
        return component switch
        {
            TextInputComponent input => [input],
            ActionRowComponent row => row.Components.SelectMany(Inputs),
            LabelComponent label => Inputs(label.Component),
            _ => []
        };
    }

    private static (string? Text, bool Ephemeral) Response(IDiscordInteraction interaction)
    {
        ICall call = Assert.Single(interaction.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IDiscordInteraction.RespondAsync));
        System.Reflection.ParameterInfo[] parameters = call.GetMethodInfo().GetParameters();
        object?[] arguments = call.GetArguments();
        return ((string?) arguments[0], (bool) arguments[Array.FindIndex(parameters, parameter => parameter.Name == "ephemeral")]!);
    }

    public void Dispose()
    {
        host.Dispose();
    }
}
