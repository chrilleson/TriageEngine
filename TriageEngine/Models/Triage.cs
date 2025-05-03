namespace TriageEngine.Models;

public record Triage(string FormId, IEnumerable<Question> Questions, IEnumerable<Result> Results);