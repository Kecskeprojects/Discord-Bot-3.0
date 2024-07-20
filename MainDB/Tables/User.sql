CREATE TABLE [dbo].[User]
(
	[UserId] INT NOT NULL IDENTITY,
    [DiscordId] VARCHAR(20) NOT NULL,
    [LastFMUsername] VARCHAR(100) NULL,
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_User_CreatedOn] DEFAULT GETDATE(), 
	[BiasGameCount] INT NOT NULL CONSTRAINT [DF_User_BiasGameCount]  DEFAULT (0),
    [ModifiedOn] DATETIME NOT NULL CONSTRAINT [DF_User_ModifiedOn] DEFAULT GETDATE(), 
    CONSTRAINT [PK_UserId] PRIMARY KEY ([UserId]),
    CONSTRAINT [UQ_UserDiscordId] UNIQUE ([DiscordId])
)
