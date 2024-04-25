CREATE TABLE [dbo].[User]
(
	[UserId] INT NOT NULL IDENTITY,
    [DiscordId] VARCHAR(20) NOT NULL,
    [LastFMUsername] VARCHAR(100) NULL,
    [CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(), 
	[BiasGameCount] INT NOT NULL DEFAULT (0),
    CONSTRAINT [PK_UserId] PRIMARY KEY ([UserId]),
    CONSTRAINT [UQ_UserDiscordId] UNIQUE ([DiscordId])
)
