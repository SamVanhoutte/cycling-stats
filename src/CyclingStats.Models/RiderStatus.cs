namespace CyclingStats.Models;

public enum RiderStatus
{
    New = 0,
    Active = 1,
    Error = -1,
    NotFound = -4,
    Retired = -2,
    WaitingForDetails = 5
}