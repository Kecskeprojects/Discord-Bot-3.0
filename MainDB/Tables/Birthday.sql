CREATE TABLE [dbo].[Birthday]
(
	[BirthdayId] INT NOT NULL IDENTITY, 
    [ServerId] INT NOT NULL, 
    [UserId] INT NOT NULL, 
    [Date] DATE NOT NULL, 
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_Birthday_CreatedOn] DEFAULT GETDATE(), 
    CONSTRAINT [FK_Birthday_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId]),
    CONSTRAINT [FK_Birthday_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]),
    CONSTRAINT [PK_BirthdayId] PRIMARY KEY ([BirthdayId])
)
