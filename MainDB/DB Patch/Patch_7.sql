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
PRINT N'Rename refactoring operation with key 2e734617-12fa-427b-a37b-86d6243eab16 is skipped, element [dbo].[EmbedGroup].[Id] (SqlSimpleColumn) will not be renamed to EmbedGroupId';


GO
PRINT N'Rename refactoring operation with key e1d6c9af-abf0-476f-8867-5a836a01e57f is skipped, element [dbo].[Embed].[Id] (SqlSimpleColumn) will not be renamed to EmbedId';


GO
PRINT N'Creating Table [dbo].[Embed]...';


GO
CREATE TABLE [dbo].[Embed] (
    [EmbedId]      INT            IDENTITY (1, 1) NOT NULL,
    [EmbedGroupId] INT            NOT NULL,
    [Order]        INT            NOT NULL,
    [ContentType]  INT            NOT NULL,
    [CreatedOn]    DATETIME       NOT NULL,
    [ModifiedOn]   DATETIME       NOT NULL,
    [ImageUrl]     NVARCHAR (MAX) NULL,
    [EmbedContent] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_EmbedId] PRIMARY KEY CLUSTERED ([EmbedId] ASC),
    CONSTRAINT [UQ_Embed] UNIQUE NONCLUSTERED ([EmbedGroupId] ASC, [Order] ASC)
);


GO
PRINT N'Creating Table [dbo].[EmbedGroup]...';


GO
CREATE TABLE [dbo].[EmbedGroup] (
    [EmbedGroupId] INT      IDENTITY (1, 1) NOT NULL,
    [ServerId]     INT      NOT NULL,
    [ChannelId]    INT      NOT NULL,
    [MessageId]    INT      NOT NULL,
    [CreatedOn]    DATETIME NOT NULL,
    CONSTRAINT [PK_EmbedGroupId] PRIMARY KEY CLUSTERED ([EmbedGroupId] ASC),
    CONSTRAINT [UQ_EmbedGroup] UNIQUE NONCLUSTERED ([ServerId] ASC, [ChannelId] ASC, [MessageId] ASC)
);


GO
PRINT N'Creating Default Constraint [dbo].[DF_Embed_ContentType]...';


GO
ALTER TABLE [dbo].[Embed]
    ADD CONSTRAINT [DF_Embed_ContentType] DEFAULT 1 FOR [ContentType];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Embed_CreatedOn]...';


GO
ALTER TABLE [dbo].[Embed]
    ADD CONSTRAINT [DF_Embed_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Embed_ModifiedOn]...';


GO
ALTER TABLE [dbo].[Embed]
    ADD CONSTRAINT [DF_Embed_ModifiedOn] DEFAULT GETDATE() FOR [ModifiedOn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_EmbedGroup_CreatedOn]...';


GO
ALTER TABLE [dbo].[EmbedGroup]
    ADD CONSTRAINT [DF_EmbedGroup_CreatedOn] DEFAULT GETDATE() FOR [CreatedOn];


GO
ALTER TABLE [dbo].[Embed] WITH NOCHECK
    ADD CONSTRAINT [FK_Embed_EmbedGroup] FOREIGN KEY ([EmbedGroupId]) REFERENCES [dbo].[EmbedGroup] ([EmbedGroupId]) ON DELETE CASCADE;


GO
PRINT N'Creating Foreign Key [dbo].[FK_EmbedGroup_Server]...';


GO
ALTER TABLE [dbo].[EmbedGroup] WITH NOCHECK
    ADD CONSTRAINT [FK_EmbedGroup_Server] FOREIGN KEY ([ServerId]) REFERENCES [dbo].[Server] ([ServerId]) ON DELETE CASCADE;


GO
PRINT N'Creating Foreign Key [dbo].[FK_EmbedGroup_Channel]...';


GO
ALTER TABLE [dbo].[EmbedGroup] WITH NOCHECK
    ADD CONSTRAINT [FK_EmbedGroup_Channel] FOREIGN KEY ([ChannelId]) REFERENCES [dbo].[Channel] ([ChannelId]) ON DELETE CASCADE;


GO
-- Refactoring step to update target server with deployed transaction logs
IF NOT EXISTS (SELECT OperationKey FROM [dbo].[__RefactorLog] WHERE OperationKey = '2e734617-12fa-427b-a37b-86d6243eab16')
INSERT INTO [dbo].[__RefactorLog] (OperationKey) values ('2e734617-12fa-427b-a37b-86d6243eab16')
IF NOT EXISTS (SELECT OperationKey FROM [dbo].[__RefactorLog] WHERE OperationKey = 'e1d6c9af-abf0-476f-8867-5a836a01e57f')
INSERT INTO [dbo].[__RefactorLog] (OperationKey) values ('e1d6c9af-abf0-476f-8867-5a836a01e57f')

GO

GO
PRINT N'Checking existing data against newly created constraints';


GO
USE [$(DatabaseName)];


GO
ALTER TABLE [dbo].[Embed] WITH CHECK CHECK CONSTRAINT [FK_Embed_EmbedGroup];

ALTER TABLE [dbo].[EmbedGroup] WITH CHECK CHECK CONSTRAINT [FK_EmbedGroup_Server];

ALTER TABLE [dbo].[EmbedGroup] WITH CHECK CHECK CONSTRAINT [FK_EmbedGroup_Channel];


GO
PRINT N'Update complete.';


GO
