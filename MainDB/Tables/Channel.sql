CREATE TABLE [dbo].[Channel]
(
	[ChannelId] INT NOT NULL IDENTITY, 
    [ServerId] INT NOT NULL, 
    [DiscordId] VARCHAR(20) NOT NULL, 
    CONSTRAINT [FK_Channel_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId]),
    CONSTRAINT [PK_ChannelId] PRIMARY KEY ([ChannelId]),
    CONSTRAINT [UQ_ChannelDiscordId] UNIQUE ([DiscordId])
)
