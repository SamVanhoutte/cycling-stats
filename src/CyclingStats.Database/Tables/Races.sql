CREATE TABLE [dbo].[Races]
(
  [Id] NVARCHAR(255) NOT NULL PRIMARY KEY,
  [StageId] NVARCHAR(64) NULL,
  [StageRaceId] NVARCHAR(64) NULL,
  [StageRace] BIT NOT NULL,
  [Name] NVARCHAR(MAX) NULL,
  [RaceDate] DATETIME NULL,
  [RaceType] NVARCHAR(20),
  [Distance] DECIMAL,
  [Status] NVARCHAR(20),
  [ProfileImageUrl] NVARCHAR(128) NULL,
  [PointsScale] NVARCHAR(128) NULL,
  [UciScale] NVARCHAR(64) NULL,
  [ParcoursType] INT NULL,
  [ProfileScore] INT NULL,
  [RaceRanking] INT NULL,
  [Elevation] INT NULL,
  [StartlistQuality] INT NULL,
  [DecidingMethod] NVARCHAR(128) NULL,
  [Classification] NVARCHAR(128) NULL,
  [Category] NVARCHAR(128) NULL
)
