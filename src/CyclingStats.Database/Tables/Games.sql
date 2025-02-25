CREATE TABLE [dbo].[Games]
(
  [RaceId] NVARCHAR(255) NOT NULL PRIMARY KEY,
  StarBudget INT NULL,
  Status INT NOT NULL,
  Updated DATETIME NOT NULL DEFAULT getdate()
)
