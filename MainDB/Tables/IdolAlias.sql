CREATE TABLE [dbo].[IdolAlias]
(
	[IdolAliasId] INT NOT NULL IDENTITY,
    [Alias] VARCHAR(100) NOT NULL, 
    [IdolId] INT NOT NULL, 
    CONSTRAINT [PK_IdolAliasId] PRIMARY KEY ([IdolAliasId]), 
    CONSTRAINT [FK_IdolAlias_Idol] FOREIGN KEY ([IdolId]) REFERENCES [Idol]([IdolId]) ON DELETE CASCADE
)
