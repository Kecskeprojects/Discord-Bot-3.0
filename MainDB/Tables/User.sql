CREATE TABLE [dbo].[User]
(
	[UserId] INT NOT NULL IDENTITY,
    [DiscordId] VARCHAR(20) NOT NULL,
    [LastFMUsername] VARCHAR(100) NULL,
    CONSTRAINT [PK_UserId] PRIMARY KEY ([UserId]),
    CONSTRAINT [UQ_UserDiscordId] UNIQUE ([DiscordId])
)
