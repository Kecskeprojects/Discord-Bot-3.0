CREATE TABLE [dbo].[Idol]
(
	[IdolId] INT NOT NULL IDENTITY, 
    [Name] VARCHAR(100) NOT NULL, 
    [GroupId] INT NOT NULL, 
    CONSTRAINT [FK_Idol_IdolGroup] FOREIGN KEY ([GroupId]) REFERENCES [IdolGroup]([GroupId]),
    CONSTRAINT [PK_IdolId] PRIMARY KEY ([IdolId])
)
