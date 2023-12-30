USE [$(DatabaseName)];

IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 1 )
	INSERT INTO [dbo].[ChannelType] VALUES (1, 'RoleText', GETDATE());
	
IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 2 )
	INSERT INTO [dbo].[ChannelType] VALUES  (2, 'TwitchNotificationText', GETDATE());

IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 3 )
	INSERT INTO [dbo].[ChannelType] VALUES  (3, 'MusicText', GETDATE());

IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 4 )
	INSERT INTO [dbo].[ChannelType] VALUES  (4, 'MusicVoice', GETDATE());

IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 5 )
	INSERT INTO [dbo].[ChannelType] VALUES  (5, 'CommandText', GETDATE());

IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 6 )
	INSERT INTO [dbo].[ChannelType] VALUES  (6, 'BirthdayText', GETDATE());