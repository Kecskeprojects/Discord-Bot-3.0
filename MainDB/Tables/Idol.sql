CREATE TABLE [dbo].[Idol]
(
	[IdolId] INT NOT NULL IDENTITY, 
    [Name] VARCHAR(100) NOT NULL, 
    [GroupId] INT NOT NULL, 
    [ProfileUrl] NVARCHAR(200) NULL, 
    [StageName] NVARCHAR(100) NULL, 
    [FullName] NVARCHAR(100) NULL, 
    [KoreanFullName] NVARCHAR(100) NULL, 
    [KoreanStageName] NVARCHAR(100) NULL, 
    [DateOfBirth] DATE NULL, 
    [Gender] VARCHAR(10) NULL, 
    [CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(), 
    [ModifiedOn] DATETIME NOT NULL DEFAULT GETDATE(), 
    [DebutDate] DATE NULL, 
    CONSTRAINT [FK_Idol_IdolGroup] FOREIGN KEY ([GroupId]) REFERENCES [IdolGroup]([GroupId]) ON DELETE CASCADE,
    CONSTRAINT [PK_IdolId] PRIMARY KEY ([IdolId])
)
