CREATE TABLE [dbo].[ServerSettingChannel]
(
    [ChannelId] INT NOT NULL, 
    [ChannelTypeId] INT NOT NULL, 
    CONSTRAINT [FK_ServerSettingChannel_Channel] FOREIGN KEY ([ChannelId]) REFERENCES [Channel]([ChannelId]), 
    CONSTRAINT [FK_ServerSettingChannel_ChannelType] FOREIGN KEY ([ChannelTypeId]) REFERENCES [ChannelType]([ChannelTypeId]) ON DELETE CASCADE,
    CONSTRAINT [PK_ChannelId_ChannelTypeId] PRIMARY KEY ([ChannelId], [ChannelTypeId])
)
