CREATE TABLE [aerobets].[UserGames]
(
  [UserId] NVARCHAR(255) NOT NULL,
  [GameId] NVARCHAR(255) NOT NULL,
  Comment NVARCHAR(MAX) NULL,
  Created DATETIME NOT NULL DEFAULT getdate(),
  CONSTRAINT PK_UserGames PRIMARY KEY (UserId, GameId)
)
