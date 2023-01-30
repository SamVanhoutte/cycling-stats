CREATE TABLE [dbo].[Riders] (
    [Id]           NVARCHAR (255) NOT NULL,
    [Name]         NVARCHAR (MAX) NOT NULL,
    [Team]         NVARCHAR (MAX) NOT NULL,
    [Sprinter]     INT            NULL,
    [Puncheur]     INT            NULL,
    [OneDay]       INT            NULL,
    [Climber]      INT            NULL,
    [AllRounder]   INT            NULL,
    [TimeTrialist] INT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
