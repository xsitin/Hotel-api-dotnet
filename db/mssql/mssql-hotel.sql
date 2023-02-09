USE [master.dbo]
GO

CREATE TABLE [Rooms] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Orders] (
    [Id] int NOT NULL IDENTITY,
    [From] datetime2 NOT NULL,
    [Until] datetime2 NOT NULL,
    [RoomId] int NULL,
    [Username] nvarchar(450) NULL,
    [IsVip] bit NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id])
);
GO


CREATE UNIQUE INDEX [IX_Orders_Id] ON [Orders] ([Id]);
GO


CREATE INDEX [IX_Orders_RoomId] ON [Orders] ([RoomId]);
GO


CREATE INDEX [IX_Orders_Username] ON [Orders] ([Username]);
GO


CREATE UNIQUE INDEX [IX_Rooms_Id] ON [Rooms] ([Id]);
GO
