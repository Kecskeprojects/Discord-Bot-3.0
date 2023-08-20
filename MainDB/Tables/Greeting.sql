CREATE TABLE [dbo].[Greeting]
(
	[GreetingId] INT NOT NULL IDENTITY, 
    [Url] VARCHAR(500) NOT NULL,
    CONSTRAINT [PK_GreetingId] PRIMARY KEY ([GreetingId])
)
