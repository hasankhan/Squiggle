CREATE DATABASE SquiggleHistory;
GO

USE SquiggleHistory;
GO

CREATE SCHEMA [HistoryModelStoreContainer];
GO

CREATE TABLE [HistoryModelStoreContainer].[Events] (
  [SessionId] uniqueidentifier NOT NULL
, [EventTypeCode] int NOT NULL
, [Sender] uniqueidentifier NOT NULL
, [Data] nvarchar(4000) NULL
, [Stamp] datetime NOT NULL
, [SenderName] nvarchar(255) NOT NULL
, [Id] uniqueidentifier NOT NULL
);
GO
CREATE TABLE [HistoryModelStoreContainer].[Participants] (
  [Id] uniqueidentifier NOT NULL
, [ParticipantId] uniqueidentifier NOT NULL
, [ParticpantName] nvarchar(255) NOT NULL
, [SessionId] uniqueidentifier NOT NULL
);
GO
CREATE TABLE [HistoryModelStoreContainer].[Sessions] (
  [Id] uniqueidentifier NOT NULL
, [Contact] uniqueidentifier NOT NULL
, [ContactName] nvarchar(255) NOT NULL
, [Start] datetime NOT NULL
, [End] datetime NULL
);
GO
ALTER TABLE [HistoryModelStoreContainer].[Events] ADD CONSTRAINT [PK__Events__00000000000000B0] PRIMARY KEY ([Id]);
GO
ALTER TABLE [HistoryModelStoreContainer].[Participants] ADD CONSTRAINT [PK_Participants] PRIMARY KEY ([Id]);
GO
ALTER TABLE [HistoryModelStoreContainer].[Sessions] ADD CONSTRAINT [PK_Sessions] PRIMARY KEY ([Id]);
GO
CREATE UNIQUE INDEX [UQ__Events__00000000000000A8] ON [HistoryModelStoreContainer].[Events] ([Id] ASC);
GO
CREATE UNIQUE INDEX [UQ__Participants__00000000000000DB] ON [HistoryModelStoreContainer].[Participants] ([Id] ASC);
GO
CREATE UNIQUE INDEX [UQ__Sessions__00000000000000C9] ON [HistoryModelStoreContainer].[Sessions] ([Id] ASC);
GO

