CREATE TABLE [aerobets].[Users]
(
  UserId NVARCHAR(255) NOT NULL PRIMARY KEY,
  Name NVARCHAR(255) NOT NULL,
  Email NVARCHAR(255) NOT NULL,
  AuthenticationId NVARCHAR(255) NULL,
  WcsUserName NVARCHAR(64) NULL,
  WcsPasswordEncrypted NVARCHAR(255) NULL,
  Phone  NVARCHAR(16) NULL,
  Language  NVARCHAR(16) NULL,
  Updated DateTime NOT NULL DEFAULT getdate()
)
