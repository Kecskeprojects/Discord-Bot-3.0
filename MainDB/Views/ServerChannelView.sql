CREATE VIEW [dbo].[ServerChannelView]
	AS SELECT 
		[Server].[ServerId] AS [ServerId],
		[Server].[DiscordId] AS [ServerDiscordId],
		[Channel].[ChannelId] AS [ChannelId],
		[Channel].[DiscordId] AS [ChannelDiscordId],
		[ChannelType].[ChannelTypeId] AS [ChannelTypeId],
		[ChannelType].[Name] AS [ChannelTypeName]
	FROM [ServerSettingChannel] AS [base]
	LEFT JOIN [dbo].[ChannelType] ON [ChannelType].[ChannelTypeId] = [base].[ChannelTypeId]
	LEFT JOIN [dbo].[Channel] ON [Channel].[ChannelId] = [base].[ChannelId]
	LEFT JOIN [dbo].[Server] ON [Server].[ServerId] = [Channel].[ServerId]

