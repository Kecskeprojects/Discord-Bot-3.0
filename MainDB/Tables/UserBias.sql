CREATE TABLE [dbo].[UserBias]
(
    [UserId] INT NOT NULL, 
    [IdolId] INT NOT NULL, 
    CONSTRAINT [FK_UserBias_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]), 
    CONSTRAINT [FK_UserBias_Idol] FOREIGN KEY ([IdolId]) REFERENCES [Idol]([IdolId]) ON DELETE CASCADE,
    CONSTRAINT [PK_UserId_IdolId] PRIMARY KEY ([UserId], [IdolId])
)
