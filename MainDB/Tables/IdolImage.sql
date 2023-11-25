﻿CREATE TABLE [dbo].[IdolImage]
(
	[ImageId] INT NOT NULL IDENTITY, 
    [IdolId] INT NOT NULL, 
    [ImageURL] VARCHAR(200) NOT NULL,
    CONSTRAINT [FK_IdolImage_Idol] FOREIGN KEY ([IdolId]) REFERENCES [Idol]([IdolId]) ON DELETE CASCADE,
    CONSTRAINT [PK_ImageId] PRIMARY KEY ([ImageId])
)
