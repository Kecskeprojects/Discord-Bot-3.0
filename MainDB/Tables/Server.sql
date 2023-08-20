CREATE TABLE [dbo].[Server]
(
	[ServerId] INT NOT NULL IDENTITY, 
    [DiscordId] VARCHAR(20) NOT NULL,
    CONSTRAINT [PK_ServerId] PRIMARY KEY ([ServerId]),
    CONSTRAINT [UQ_ServerDiscordId] UNIQUE ([DiscordId])
)
