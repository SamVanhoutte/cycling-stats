CREATE TABLE [dbo].[RacePoints]
(
  [RaceId] NVARCHAR(255) NOT NULL,
  [RiderId] NVARCHAR(255) NOT NULL,
  [Points] INT NOT NULL,
  [Position] INT NOT NULL,
  [Gc] INT NULL,
  [Pc] INT NULL,
  [Mc] INT NULL,
  [Picked] DECIMAL NOT NULL,
  [Stars] INT NOT NULL,
  PRIMARY KEY (RaceId, RiderId)
)
