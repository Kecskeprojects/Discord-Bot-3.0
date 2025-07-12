CREATE TABLE [dbo].[WeeklyPollOptionPreset]
(
	[WeeklyPollOptionPresetId] INT NOT NULL IDENTITY, 
    [Name] NVARCHAR(100) NOT NULL, 
    [Description] NVARCHAR(100) NOT NULL, 
    [IsSpecialPreset] BIT NOT NULL CONSTRAINT [DF_WeeklyPollOptionPreset_IsSpecialPreset] DEFAULT 0, 
    [IsActive] BIT NOT NULL CONSTRAINT [DF_WeeklyPollOptionPreset_IsActive] DEFAULT 1, 
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_WeeklyPollOptionPreset_CreatedOn] DEFAULT GETUTCDATE(), 
    [ModifiedOn] DATETIME NOT NULL CONSTRAINT [DF_WeeklyPollOptionPreset_ModifiedOn] DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_WeeklyPollOptionPresetId] PRIMARY KEY ([WeeklyPollOptionPresetId])
)
