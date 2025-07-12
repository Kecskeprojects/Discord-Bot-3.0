CREATE TABLE [dbo].[UserIdolStatistic]
(
    [UserId] INT NOT NULL, 
    [IdolId] INT NOT NULL, 
    [Placed1] INT NOT NULL CONSTRAINT [DF_UserIdolStatistic_Place1] DEFAULT 0, 
    [Placed2] INT NOT NULL CONSTRAINT [DF_UserIdolStatistic_Place2] DEFAULT 0, 
    [Placed3] INT NOT NULL CONSTRAINT [DF_UserIdolStatistic_Place3] DEFAULT 0, 
    [Placed4] INT NOT NULL CONSTRAINT [DF_UserIdolStatistic_Place4] DEFAULT 0, 
    [Placed5] INT NOT NULL CONSTRAINT [DF_UserIdolStatistic_Place5] DEFAULT 0,
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_UseIdolStatistic_CreatedOn] DEFAULT GETUTCDATE(),
    [ModifiedOn] DATETIME NOT NULL CONSTRAINT [DF_UseIdolStatistic_ModifiedOn] DEFAULT GETUTCDATE(), 
    CONSTRAINT [FK_UserIdolStatistic_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]), 
    CONSTRAINT [FK_UserIdolStatistic_Idol] FOREIGN KEY ([IdolId]) REFERENCES [Idol]([IdolId]) ON DELETE CASCADE,
    CONSTRAINT [PK_UserId_IdolId_UserIdolStatistic] PRIMARY KEY ([UserId], [IdolId])
)
