CREATE TABLE [dbo].[Riders]
(
  [Id] NVARCHAR(255) NOT NULL PRIMARY KEY,
  [Team] NVARCHAR(MAX) NOT NULL,
  [Sprinter] INT,
  [Puncheur] INT,
  [OneDay] INT,
  [Climber] INT,
  AllRounder INT,
  TimeTrialist INT 
)
