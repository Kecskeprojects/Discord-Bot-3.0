CREATE TABLE [dbo].[Role]
(
	[RoleId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ServerId] INT NOT NULL, 
    [DiscordId] VARCHAR(20) NOT NULL UNIQUE,
    CONSTRAINT [FK_Role_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId])
)
