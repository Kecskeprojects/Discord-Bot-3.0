CREATE TABLE [dbo].[WeeklyPollOption]
(
	[WeeklyPollOptionId] INT NOT NULL IDENTITY, 
    [WeeklyPollId] INT NULL, 
    [WeeklyPollOptionPresetId] INT NULL, 
    [OrderNumber] TINYINT NOT NULL, 
    [Title] NVARCHAR(55) NOT NULL, 
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_WeeklyPollOption_CreatedOn] DEFAULT GETUTCDATE(), 
    [ModifiedOn] DATETIME NOT NULL CONSTRAINT [DF_WeeklyPollOption_ModifiedOn] DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_WeeklyPollOptionId] PRIMARY KEY ([WeeklyPollOptionId]),
    CONSTRAINT [FK_WeeklyPollOption_WeeklyPollId] FOREIGN KEY ([WeeklyPollId]) REFERENCES [WeeklyPoll]([WeeklyPollId]),
    CONSTRAINT [FK_WeeklyPollOption_WeeklyPollOptionPresetId] FOREIGN KEY ([WeeklyPollOptionPresetId]) REFERENCES [WeeklyPollOptionPreset]([WeeklyPollOptionPresetId]),
    CONSTRAINT [CK_WeeklyPollOption_OrderNumber] CHECK ([OrderNumber] >= 0 AND [OrderNumber] < 10), 
)
