CREATE TABLE [dbo].[Keyword]
(
	[KeywordId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ServerId] INT NOT NULL, 
    [Trigger] VARCHAR(100) NOT NULL, 
    [Response] VARCHAR(300) NOT NULL, 
    CONSTRAINT [FK_Keyword_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId]) 
)
