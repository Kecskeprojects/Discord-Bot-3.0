CREATE TABLE [dbo].[IdolGroup]
(
	[GroupId] INT NOT NULL IDENTITY, 
    [Name] VARCHAR(100) NOT NULL,
    [DebutDate] DATE NULL, 
    [FullName] NVARCHAR(100) NULL, 
    [FullKoreanName] NVARCHAR(100) NULL, 
    [CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(), 
    [ModifiedOn] DATETIME NOT NULL DEFAULT GETDATE(), 
    CONSTRAINT [PK_GroupId] PRIMARY KEY ([GroupId])
)
