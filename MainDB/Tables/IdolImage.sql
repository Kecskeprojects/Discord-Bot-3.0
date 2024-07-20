﻿CREATE TABLE [dbo].[IdolImage]
(
	[ImageId] INT NOT NULL IDENTITY, 
    [IdolId] INT NOT NULL, 
    [ImageURL] NVARCHAR(200) NOT NULL,
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_IdolImage_CreatedOn] DEFAULT GETDATE(), 
    [OverriddenURL] NVARCHAR(200) NULL, 
    CONSTRAINT [FK_IdolImage_Idol] FOREIGN KEY ([IdolId]) REFERENCES [Idol]([IdolId]) ON DELETE CASCADE,
    CONSTRAINT [PK_ImageId] PRIMARY KEY ([ImageId])
)
