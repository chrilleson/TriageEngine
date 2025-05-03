namespace TriageEngine.Models;

public record Rule(string? Condition, string? Action, int? GotoQuestionId, int? GotoResultId);