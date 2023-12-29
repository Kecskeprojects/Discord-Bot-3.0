CREATE TABLE [dbo].[UserIdolStatistic]
(
    [UserId] INT NOT NULL, 
    [IdolId] INT NOT NULL, 
    [Placed1] INT NOT NULL DEFAULT 0, 
    [Placed2] INT NOT NULL DEFAULT 0, 
    [Placed3] INT NOT NULL DEFAULT 0, 
    [Placed4] INT NOT NULL DEFAULT 0, 
    [Placed5] INT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_UserIdolStatistic_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]), 
    CONSTRAINT [FK_UserIdolStatistic_Idol] FOREIGN KEY ([IdolId]) REFERENCES [Idol]([IdolId]) ON DELETE CASCADE,
    CONSTRAINT [PK_UserId_IdolId_UserIdolStatistic] PRIMARY KEY ([UserId], [IdolId])
)
