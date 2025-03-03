CREATE TABLE aerobets.UserTeams
(
    [TeamId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [UserId] NVARCHAR(255) NOT NULL,    
    [GameId] NVARCHAR(255) NOT NULL,
    Name NVARCHAR(255) NULL,
    Comment NVARCHAR(512) NULL,
    TeamListJson NVARCHAR(MAX) NULL,
    Created DATETIME NOT NULL DEFAULT getdate()
)