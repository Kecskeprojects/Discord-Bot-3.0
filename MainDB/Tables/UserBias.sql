﻿CREATE TABLE [dbo].[UserBias]
(
	[UserBiasId] INT NOT NULL IDENTITY, 
    [UserId] INT NOT NULL, 
    [IdolId] INT NOT NULL, 
    CONSTRAINT [FK_UserBias_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]), 
    CONSTRAINT [FK_UserBias_Idol] FOREIGN KEY ([IdolId]) REFERENCES [Idol]([IdolId]),
    CONSTRAINT [PK_UserBiasId] PRIMARY KEY ([UserBiasId])
)
