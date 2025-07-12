CREATE TABLE [dbo].[Role]
(
	[RoleId] INT NOT NULL IDENTITY, 
    [ServerId] INT NOT NULL, 
    [DiscordId] VARCHAR(20) NOT NULL,
    [RoleName] VARCHAR(50) NOT NULL, 
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_Role_CreatedOn] DEFAULT GETUTCDATE(), 
    CONSTRAINT [FK_Role_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId]),
    CONSTRAINT [PK_RoleId] PRIMARY KEY ([RoleId]),
    CONSTRAINT [UQ_RoleDiscordId] UNIQUE ([DiscordId])
)
