﻿CREATE TABLE [dbo].[TwitchChannel]
(
	[TwitchChannelId] INT NOT NULL IDENTITY, 
    [TwitchId] VARCHAR(12) NOT NULL, 
    [TwitchLink] VARCHAR(100) NOT NULL, 
    [TwitchName] VARCHAR(75) NOT NULL, 
    [ServerId] INT NOT NULL,
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_TwitchChannel_CreatedOn] DEFAULT GETUTCDATE(), 
    CONSTRAINT [FK_TwitchChannel_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId]),
    CONSTRAINT [PK_TwitchChannelId] PRIMARY KEY ([TwitchChannelId])
)
