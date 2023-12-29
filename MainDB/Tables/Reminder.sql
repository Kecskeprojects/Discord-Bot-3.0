CREATE TABLE [dbo].[Reminder]
(
	[ReminderId] INT NOT NULL IDENTITY, 
    [UserId] INT NOT NULL, 
    [Date] DATETIME2 NOT NULL, 
    [Message] VARCHAR(500) NOT NULL, 
    [CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(), 
    CONSTRAINT [FK_Reminder_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]),
    CONSTRAINT [PK_ReminderId] PRIMARY KEY ([ReminderId])
)
