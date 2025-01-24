CREATE TABLE [dbo].[RacePoints]
(
  [RaceId] NVARCHAR(255) NOT NULL,
  [RiderId] NVARCHAR(255) NOT NULL,
  [Points] INT NOT NULL,
  [Position] INT NOT NULL,
  [Gc] INT NOT NULL,
  [Pc] INT NOT NULL,
  [Mc] INT NOT NULL,
  [Picked] DECIMAL NOT NULL,
  [Stars] INT NOT NULL,
  PRIMARY KEY (RaceId, RiderId)
)
