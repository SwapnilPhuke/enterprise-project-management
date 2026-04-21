namespace ProjectManagement.Models;

/// <summary>
/// Represents the workflow stage of a project.
/// Integer values are persisted in the database and used in the frontend progress bar.
/// </summary>
public enum ProjectStatus
{
    NotStarted = 0,
    InProgress = 25,
    Testing    = 50,
    Review     = 75,
    Completed  = 100
}
