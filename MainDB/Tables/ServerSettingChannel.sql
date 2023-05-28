CREATE TABLE [dbo].[ServerSettingChannel]
(
	[ServerSettingChannelId] INT NOT NULL PRIMARY KEY IDENTITY,
    [ChannelId] INT NOT NULL, 
    [ChannelTypeId] INT NOT NULL, 
    CONSTRAINT [FK_ServerSettingChannel_Channel] FOREIGN KEY ([ChannelId]) REFERENCES [Channel]([ChannelId]), 
    CONSTRAINT [FK_ServerSettingChannel_ChannelType] FOREIGN KEY ([ChannelTypeId]) REFERENCES [ChannelType]([ChannelTypeId]),
)
