CREATE TABLE [dbo].[IdolGroup]
(
	[GroupId] INT NOT NULL IDENTITY, 
    [Name] VARCHAR(100) NOT NULL,
    [DebutDate] DATE NULL, 
    [FullName] NVARCHAR(100) NULL, 
    [FullKoreanName] NVARCHAR(100) NULL, 
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_IdolGroup_CreatedOn] DEFAULT GETUTCDATE(), 
    [ModifiedOn] DATETIME NOT NULL CONSTRAINT [DF_IdolGroup_ModifiedOn] DEFAULT GETUTCDATE(), 
    CONSTRAINT [PK_GroupId] PRIMARY KEY ([GroupId])
)
