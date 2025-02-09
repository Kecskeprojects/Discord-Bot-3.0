﻿/*
Deployment script for MainDB

This code was generated by a tool.
Changes to this file may cause incorrect behavior and will be lost if
the code is regenerated.
*/

GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;


GO
:setvar BuildConfiguration "Release"
:setvar DatabaseName "MainDB"
:setvar DefaultFilePrefix "MainDB"
:setvar DefaultDataPath "C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\"
:setvar DefaultLogPath "C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\"

GO
:on error exit
GO
/*
Detect SQLCMD mode and disable script execution if SQLCMD mode is not supported.
To re-enable the script after enabling SQLCMD mode, execute the following:
SET NOEXEC OFF; 
*/
:setvar __IsSqlCmdEnabled "True"
GO
IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'
    BEGIN
        PRINT N'SQLCMD mode must be enabled to successfully execute this script.';
        SET NOEXEC ON;
    END


GO
USE [$(DatabaseName)];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[Birthday]...';


GO
ALTER TABLE [dbo].[Birthday] DROP CONSTRAINT [DF__Birthday__Create__6E01572D];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[Channel]...';


GO
ALTER TABLE [dbo].[Channel] DROP CONSTRAINT [DF__Channel__Created__6EF57B66];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[ChannelType]...';


GO
ALTER TABLE [dbo].[ChannelType] DROP CONSTRAINT [DF__ChannelTy__Creat__6FE99F9F];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[CustomCommand]...';


GO
ALTER TABLE [dbo].[CustomCommand] DROP CONSTRAINT [DF__CustomCom__Creat__70DDC3D8];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[Greeting]...';


GO
ALTER TABLE [dbo].[Greeting] DROP CONSTRAINT [DF__Greeting__Create__71D1E811];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[Idol]...';


GO
ALTER TABLE [dbo].[Idol] DROP CONSTRAINT [DF__Idol__CreatedOn__72C60C4A];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[Idol]...';


GO
ALTER TABLE [dbo].[Idol] DROP CONSTRAINT [DF__Idol__ModifiedOn__73BA3083];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[IdolAlias]...';


GO
ALTER TABLE [dbo].[IdolAlias] DROP CONSTRAINT [DF__IdolAlias__Creat__74AE54BC];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[IdolGroup]...';


GO
ALTER TABLE [dbo].[IdolGroup] DROP CONSTRAINT [DF__IdolGroup__Creat__75A278F5];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[IdolGroup]...';


GO
ALTER TABLE [dbo].[IdolGroup] DROP CONSTRAINT [DF__IdolGroup__Modif__76969D2E];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[IdolImage]...';


GO
ALTER TABLE [dbo].[IdolImage] DROP CONSTRAINT [DF__IdolImage__Creat__778AC167];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[Keyword]...';


GO
ALTER TABLE [dbo].[Keyword] DROP CONSTRAINT [DF__Keyword__Created__787EE5A0];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[Reminder]...';


GO
ALTER TABLE [dbo].[Reminder] DROP CONSTRAINT [DF__Reminder__Create__797309D9];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[Role]...';


GO
ALTER TABLE [dbo].[Role] DROP CONSTRAINT [DF__Role__CreatedOn__7A672E12];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[Server]...';


GO
ALTER TABLE [dbo].[Server] DROP CONSTRAINT [DF__Server__CreatedO__7B5B524B];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[TwitchChannel]...';


GO
ALTER TABLE [dbo].[TwitchChannel] DROP CONSTRAINT [DF__TwitchCha__Creat__7C4F7684];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[User]...';


GO
ALTER TABLE [dbo].[User] DROP CONSTRAINT [DF__User__CreatedOn__7D439ABD];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[User]...';


GO
ALTER TABLE [dbo].[User] DROP CONSTRAINT [DF__User__BiasGameCo__06CD04F7];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[UserIdolStatistic]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic] DROP CONSTRAINT [DF__UserIdolS__Place__00200768];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[UserIdolStatistic]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic] DROP CONSTRAINT [DF__UserIdolS__Place__01142BA1];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[UserIdolStatistic]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic] DROP CONSTRAINT [DF__UserIdolS__Place__02084FDA];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[UserIdolStatistic]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic] DROP CONSTRAINT [DF__UserIdolS__Place__02FC7413];


GO
PRINT N'Dropping Default Constraint unnamed constraint on [dbo].[UserIdolStatistic]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic] DROP CONSTRAINT [DF__UserIdolS__Place__03F0984C];


GO
PRINT N'Altering Table [dbo].[Server]...';


GO
ALTER TABLE [dbo].[Server]
    ADD [ModifiedOn] DATETIME CONSTRAINT [DF_Server_ModifiedOn] DEFAULT GETDATE() NOT NULL;


GO
PRINT N'Altering Table [dbo].[User]...';


GO
ALTER TABLE [dbo].[User]
    ADD [ModifiedOn] DATETIME CONSTRAINT [DF_User_ModifiedOn] DEFAULT GETDATE() NOT NULL;


GO
PRINT N'Altering Table [dbo].[UserIdolStatistic]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic]
    ADD [CreatedOn]  DATETIME CONSTRAINT [DF_UseIdolStatistic_CreatedOn] DEFAULT GETDATE() NOT NULL,
        [ModifiedOn] DATETIME CONSTRAINT [DF_UseIdolStatistic_ModifiedOn] DEFAULT GETDATE() NOT NULL;


GO
PRINT N'Creating Default Constraint [dbo].[DF_Birthday_CreatedOn]...';


GO
ALTER TABLE [dbo].[Birthday]
    ADD CONSTRAINT [DF_Birthday_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Channel_CreatedOn]...';


GO
ALTER TABLE [dbo].[Channel]
    ADD CONSTRAINT [DF_Channel_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_ChannelType_CreatedOn]...';


GO
ALTER TABLE [dbo].[ChannelType]
    ADD CONSTRAINT [DF_ChannelType_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CustomCommand_CreatedOn]...';


GO
ALTER TABLE [dbo].[CustomCommand]
    ADD CONSTRAINT [DF_CustomCommand_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Greeting_CreatedOn]...';


GO
ALTER TABLE [dbo].[Greeting]
    ADD CONSTRAINT [DF_Greeting_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Idol_CreatedOn]...';


GO
ALTER TABLE [dbo].[Idol]
    ADD CONSTRAINT [DF_Idol_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Idol_ModifiedOn]...';


GO
ALTER TABLE [dbo].[Idol]
    ADD CONSTRAINT [DF_Idol_ModifiedOn] DEFAULT GETDATE() FOR [ModifiedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_IdolAlias_CreatedOn]...';


GO
ALTER TABLE [dbo].[IdolAlias]
    ADD CONSTRAINT [DF_IdolAlias_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_IdolGroup_CreatedOn]...';


GO
ALTER TABLE [dbo].[IdolGroup]
    ADD CONSTRAINT [DF_IdolGroup_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_IdolGroup_ModifiedOn]...';


GO
ALTER TABLE [dbo].[IdolGroup]
    ADD CONSTRAINT [DF_IdolGroup_ModifiedOn] DEFAULT GETDATE() FOR [ModifiedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_IdolImage_CreatedOn]...';


GO
ALTER TABLE [dbo].[IdolImage]
    ADD CONSTRAINT [DF_IdolImage_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Keyword_CreatedOn]...';


GO
ALTER TABLE [dbo].[Keyword]
    ADD CONSTRAINT [DF_Keyword_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Reminder_CreatedOn]...';


GO
ALTER TABLE [dbo].[Reminder]
    ADD CONSTRAINT [DF_Reminder_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Role_CreatedOn]...';


GO
ALTER TABLE [dbo].[Role]
    ADD CONSTRAINT [DF_Role_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Server_CreatedOn]...';


GO
ALTER TABLE [dbo].[Server]
    ADD CONSTRAINT [DF_Server_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_TwitchChannel_CreatedOn]...';


GO
ALTER TABLE [dbo].[TwitchChannel]
    ADD CONSTRAINT [DF_TwitchChannel_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_User_BiasGameCount]...';


GO
ALTER TABLE [dbo].[User]
    ADD CONSTRAINT [DF_User_BiasGameCount] DEFAULT (0) FOR [BiasGameCount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_User_CreatedOn]...';


GO
ALTER TABLE [dbo].[User]
    ADD CONSTRAINT [DF_User_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_UserIdolStatistic_Place1]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic]
    ADD CONSTRAINT [DF_UserIdolStatistic_Place1] DEFAULT 0 FOR [Placed1];


GO
PRINT N'Creating Default Constraint [dbo].[DF_UserIdolStatistic_Place2]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic]
    ADD CONSTRAINT [DF_UserIdolStatistic_Place2] DEFAULT 0 FOR [Placed2];


GO
PRINT N'Creating Default Constraint [dbo].[DF_UserIdolStatistic_Place3]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic]
    ADD CONSTRAINT [DF_UserIdolStatistic_Place3] DEFAULT 0 FOR [Placed3];


GO
PRINT N'Creating Default Constraint [dbo].[DF_UserIdolStatistic_Place4]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic]
    ADD CONSTRAINT [DF_UserIdolStatistic_Place4] DEFAULT 0 FOR [Placed4];


GO
PRINT N'Creating Default Constraint [dbo].[DF_UserIdolStatistic_Place5]...';


GO
ALTER TABLE [dbo].[UserIdolStatistic]
    ADD CONSTRAINT [DF_UserIdolStatistic_Place5] DEFAULT 0 FOR [Placed5];


GO
PRINT N'Refreshing View [dbo].[ServerChannelView]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[ServerChannelView]';


GO
-- Refactoring step to update target server with deployed transaction logs
IF NOT EXISTS (SELECT OperationKey FROM [dbo].[__RefactorLog] WHERE OperationKey = '2e734617-12fa-427b-a37b-86d6243eab16')
INSERT INTO [dbo].[__RefactorLog] (OperationKey) values ('2e734617-12fa-427b-a37b-86d6243eab16')
IF NOT EXISTS (SELECT OperationKey FROM [dbo].[__RefactorLog] WHERE OperationKey = 'e1d6c9af-abf0-476f-8867-5a836a01e57f')
INSERT INTO [dbo].[__RefactorLog] (OperationKey) values ('e1d6c9af-abf0-476f-8867-5a836a01e57f')

GO
PRINT N'Update complete.';


GO
