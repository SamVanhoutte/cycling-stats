CREATE TABLE [aerobets].[UserRiders]
(
  [UserId] NVARCHAR(255) NOT NULL,
  [RiderId] NVARCHAR(255) NOT NULL,
  Comment NVARCHAR(MAX) NULL,
  Created DATETIME NOT NULL DEFAULT getdate(),
  CONSTRAINT PK_UserRiders PRIMARY KEY (UserId, RiderId)
)
