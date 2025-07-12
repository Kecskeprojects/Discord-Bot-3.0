CREATE TABLE [dbo].[Embed]
(
	[EmbedId] INT NOT NULL IDENTITY, 
    [EmbedGroupId] INT NOT NULL, 
    [Order] INT NOT NULL,
    [ContentType] INT NOT NULL CONSTRAINT [DF_Embed_ContentType]  DEFAULT 1, 
    [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_Embed_CreatedOn] DEFAULT GETUTCDATE(),
    [ModifiedOn] DATETIME NOT NULL CONSTRAINT [DF_Embed_ModifiedOn] DEFAULT GETUTCDATE(), 
    [ImageUrl] NVARCHAR(MAX) NULL, 
    [EmbedContent] NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_EmbedId] PRIMARY KEY ([EmbedId]),
    CONSTRAINT [FK_Embed_EmbedGroup] FOREIGN KEY ([EmbedGroupId]) REFERENCES [EmbedGroup]([EmbedGroupId]) ON DELETE CASCADE,
    CONSTRAINT [UQ_Embed] UNIQUE ([EmbedGroupId], [Order])

)
