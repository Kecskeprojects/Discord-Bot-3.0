CREATE TABLE [dbo].[WeeklyPollOptionPreset]
(
	[WeeklyPollOptionPresetId] INT NOT NULL IDENTITY, 
    [Name] NVARCHAR(100) NOT NULL, 
    [Description] NVARCHAR(100) NOT NULL, 
    [IsSpecialPreset] BIT NOT NULL CONSTRAINT [DF_WeeklyPollOptionPreset_IsSpecialPreset] DEFAULT 0, 
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_WeeklyPollOptionPreset_CreatedOn] DEFAULT GETDATE(), 
    [ModifiedOn] DATETIME NOT NULL CONSTRAINT [DF_WeeklyPollOptionPreset_ModifiedOn] DEFAULT GETDATE(),
    CONSTRAINT [PK_WeeklyPollOptionPresetId] PRIMARY KEY ([WeeklyPollOptionPresetId])
)
