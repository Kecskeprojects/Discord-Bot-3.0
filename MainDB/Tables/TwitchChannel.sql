CREATE TABLE [dbo].[TwitchChannel]
(
	[TwitchChannelId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TwitchId] VARCHAR(12) NOT NULL, 
    [TwitchLink] VARCHAR(100) NOT NULL, 
    [ServerId] INT NOT NULL,
    [RoleId] INT NULL, 
    CONSTRAINT [FK_TwitchChannel_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId]),
    CONSTRAINT [FK_TwitchChannel_Role] FOREIGN KEY ([RoleId]) REFERENCES [Role]([RoleId])
)
