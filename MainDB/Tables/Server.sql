CREATE TABLE [dbo].[Server]
(
	[ServerId] INT NOT NULL IDENTITY, 
    [DiscordId] VARCHAR(20) NOT NULL,
    [NotificationRoleId] INT NULL, 
    [RoleMessageDiscordId] VARCHAR(20) NULL, 
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_Server_CreatedOn] DEFAULT GETDATE(), 
    [ModifiedOn] DATETIME NOT NULL CONSTRAINT [DF_Server_ModifiedOn] DEFAULT GETDATE(), 
    [MuteRoleId] INT NULL, 
    CONSTRAINT [PK_ServerId] PRIMARY KEY ([ServerId]),
    CONSTRAINT [UQ_ServerDiscordId] UNIQUE ([DiscordId]),
    CONSTRAINT [FK_Server_NotificationRole] FOREIGN KEY ([NotificationRoleId]) REFERENCES [Role]([RoleId]),
    CONSTRAINT [FK_Server_MuteRole] FOREIGN KEY ([MuteRoleId]) REFERENCES [Role]([RoleId])
)
