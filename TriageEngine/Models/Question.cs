namespace TriageEngine.Models;

public record Question(int Id, string Text, QuestionType Type, Dictionary<int, string>? Options, string? Answer, IEnumerable<Rule>? Rules);