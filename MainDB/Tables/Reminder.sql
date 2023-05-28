CREATE TABLE [dbo].[Reminder]
(
	[ReminderId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NOT NULL, 
    [Date] DATETIME2 NOT NULL, 
    [Message] VARCHAR(500) NOT NULL, 
    CONSTRAINT [FK_Reminder_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId])
)
