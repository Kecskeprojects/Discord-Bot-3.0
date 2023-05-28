CREATE TABLE [dbo].[Idol]
(
	[IdolId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(100) NOT NULL, 
    [GroupId] INT NOT NULL, 
    CONSTRAINT [FK_Idol_IdolGroup] FOREIGN KEY ([GroupId]) REFERENCES [IdolGroup]([GroupId])
)
