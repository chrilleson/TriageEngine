namespace TriageEngine.Models;

public record Triage(string FormId, int FirstQuestionId, IEnumerable<Question> Questions, IEnumerable<Result> Results);