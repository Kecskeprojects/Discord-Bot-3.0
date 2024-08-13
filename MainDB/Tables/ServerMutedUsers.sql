CREATE TABLE [dbo].[ServerMutedUsers]
(
	[ServerId] INT NOT NULL IDENTITY, 
    [UserId] INT NOT NULL, 
    [MutedUntil] DATETIME NOT NULL,
    [RemovedRoleDiscordIds] NVARCHAR(MAX) NOT NULL, 
    CONSTRAINT [FK_ServerMutedUsers_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId]), 
    CONSTRAINT [FK_ServerMutedUsers_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]),
    CONSTRAINT [PK_Muted_ServerId_UserId] PRIMARY KEY ([UserId], [ServerId])
)
