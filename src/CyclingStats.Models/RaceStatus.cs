namespace CyclingStats.Models;

public enum RaceStatus
{
    New = 0,
    Planned = 1,
    // WaitingForStartList = 1,
    // WaitingForResults = 2,
    Finished = 3,
    Canceled = 4,
    Error = -1,
    NotFound = -4,
    // NoResultsAvailable = -2,
    // WaitingForDetails = 5
}