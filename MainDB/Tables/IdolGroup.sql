CREATE TABLE [dbo].[IdolGroup]
(
	[GroupId] INT NOT NULL IDENTITY, 
    [Name] VARCHAR(100) NOT NULL,
    [DebutDate] DATE NULL, 
    [FullName] NVARCHAR(100) NULL, 
    [FullKoreanName] NVARCHAR(100) NULL, 
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_IdolGroup_CreatedOn] DEFAULT GETDATE(), 
    [ModifiedOn] DATETIME NOT NULL CONSTRAINT [DF_IdolGroup_ModifiedOn] DEFAULT GETDATE(), 
    CONSTRAINT [PK_GroupId] PRIMARY KEY ([GroupId])
)
