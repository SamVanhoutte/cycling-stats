CREATE TABLE [dbo].[StartGrids]
(
  RiderId NVARCHAR(255) NOT NULL,
  RaceId NVARCHAR(255) NOT NULL,
  Stars INT NOT NULL,
  RiderType INT NOT NULL,
  Youth BIT NOT NULL,
  Created DATETIME NOT NULL DEFAULT getdate(),
  CONSTRAINT PK_StartGrid PRIMARY KEY (RiderId, RaceId)
);