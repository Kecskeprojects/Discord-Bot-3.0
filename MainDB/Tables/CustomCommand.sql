CREATE TABLE [dbo].[CustomCommand]
(
	[CommandId] INT NOT NULL IDENTITY, 
    [ServerId] INT NOT NULL, 
    [Command] VARCHAR(50) NOT NULL, 
    [Url] VARCHAR(500) NOT NULL, 
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_CustomCommand_CreatedOn] DEFAULT GETDATE(), 
    CONSTRAINT [FK_CustomCommand_Server] FOREIGN KEY ([ServerId]) REFERENCES [Server]([ServerId]),
    CONSTRAINT [PK_CommandId] PRIMARY KEY ([CommandId])
)
