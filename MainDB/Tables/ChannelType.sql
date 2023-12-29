CREATE TABLE [dbo].[ChannelType]
(
	[ChannelTypeId] INT NOT NULL, 
    [Name] VARCHAR(100) NOT NULL,
    [CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(), 
    CONSTRAINT [PK_ChannelTypeId] PRIMARY KEY ([ChannelTypeId])
)
