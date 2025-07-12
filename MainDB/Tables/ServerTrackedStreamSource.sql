CREATE TABLE [dbo].[ServerTrackedStreamSource]
(
	[ServerTrackedStreamSourceId] INT NOT NULL IDENTITY, 
    [SourceSite] VARCHAR(100) NOT NULL, 
    [SourceLink] VARCHAR(300) NOT NULL, 
    [SourceUsername] VARCHAR(150) NOT NULL, 
    [ServerId] INT NOT NULL,
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_ServerTrackedStreamSource_CreatedOn] DEFAULT GETUTCDATE(), 
    [TwitchId] VARCHAR(12) NOT NULL, 
    CONSTRAINT [FK_ServerTrackedStreamSource_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId]),
    CONSTRAINT [PK_ServerTrackedStreamSourceId] PRIMARY KEY ([ServerTrackedStreamSourceId])
)
