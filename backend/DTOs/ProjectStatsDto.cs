namespace ProjectManagement.DTOs;

/// <summary>Aggregated statistics for a user's project portfolio.</summary>
public record ProjectStatsDto(
    int    Total,
    int    NotStarted,
    int    InProgress,
    int    Testing,
    int    Review,
    int    Completed,
    double CompletionRate
);
