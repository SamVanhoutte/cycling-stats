CREATE TABLE [dbo].[RiderProfiles] (
    [RiderId]      NVARCHAR (255) NOT NULL,
    [Month]        NVARCHAR(4)    NOT NULL,
    [GC]     INT            NULL,
    [Sprinter]     INT            NULL,
    [Puncheur]     INT            NULL,
    [OneDay]       INT            NULL,
    [Climber]      INT            NULL,
    [TimeTrialist] INT            NULL,
    [UciRanking] INT            NULL,
    [PcsRanking] INT            NULL,
    PRIMARY KEY CLUSTERED ([RiderId], [Month] ASC)
);
