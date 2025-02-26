CREATE TABLE [aerobets].[PlayerWatchlists]
(
  [UserId] NVARCHAR(255) NOT NULL,
  Name NVARCHAR(56) NULL,
  WcsUserName NVARCHAR(56) NOT NULL,
  MainOpponent BIT NOT NULL,
  [PlayerUserId] NVARCHAR(255) NULL,
  Comment NVARCHAR(MAX),
  Created DATETIME NOT NULL DEFAULT getdate(),
  CONSTRAINT PK_PlayerWatchlists PRIMARY KEY (UserId, WcsUserName)
)
