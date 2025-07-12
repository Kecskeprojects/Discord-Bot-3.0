CREATE TABLE [dbo].[EmbedGroup]
(
	[EmbedGroupId] INT NOT NULL IDENTITY,
	[ServerId] INT NOT NULL,
	[ChannelId] INT NOT NULL,
	[MessageId] INT NOT NULL,
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_EmbedGroup_CreatedOn] DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_EmbedGroupId] PRIMARY KEY ([EmbedGroupId]),
    CONSTRAINT [FK_EmbedGroup_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId]) ON DELETE CASCADE, 
    CONSTRAINT [FK_EmbedGroup_Channel] FOREIGN KEY ([ChannelId]) REFERENCES [Channel]([ChannelId]) ON DELETE CASCADE, 
	CONSTRAINT [UQ_EmbedGroup] UNIQUE ([ServerId], [ChannelId], [MessageId]),
)
