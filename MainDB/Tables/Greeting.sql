CREATE TABLE [dbo].[Greeting]
(
	[GreetingId] INT NOT NULL IDENTITY, 
    [Url] VARCHAR(500) NOT NULL,
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_Greeting_CreatedOn] DEFAULT GETUTCDATE(), 
    CONSTRAINT [PK_GreetingId] PRIMARY KEY ([GreetingId])
)
