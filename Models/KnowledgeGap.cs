namespace Ai_Fund.Models;

public class KnowledgeGap
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string DetectedIntent { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public int OccurrenceCount { get; set; } = 1;
    public DateTime LastAsked { get; set; }
    public string Status { get; set; } = "New"; // New / Reviewing / Resolved
    public string? SuggestedAnswer { get; set; }
    public DateTime CreatedAt { get; set; }
}
