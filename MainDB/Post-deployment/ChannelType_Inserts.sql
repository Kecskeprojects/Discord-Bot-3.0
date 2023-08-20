USE [$(DatabaseName)];

IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 1 )
	INSERT INTO [dbo].[ChannelType] VALUES (1, 'RoleText');
	
IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 2 )
	INSERT INTO [dbo].[ChannelType] VALUES  (2, 'TwitchNotificationText');

IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 3 )
	INSERT INTO [dbo].[ChannelType] VALUES  (3, 'MusicText');

IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 4 )
	INSERT INTO [dbo].[ChannelType] VALUES  (4, 'MusicVoice');

IF NOT EXISTS ( SELECT * FROM [dbo].[ChannelType] WHERE [ChannelTypeId] = 5 )
	INSERT INTO [dbo].[ChannelType] VALUES  (5, 'CommandText');