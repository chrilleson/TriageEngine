using Rule = TriageEngine.Models.Rule;

namespace TriageEngine.Test;

public class RuleTests
{
    [Fact]
    public void EvaluateAnswer_ReturnsTrue_WhenConditionIsMet()
    {
        var rule = CreateSut("x > 5");
        const int answer = 10;

        var result = Rule.EvaluateAnswer(rule.Condition!, answer);

        result.ShouldBeTrue();
    }

    private static Rule CreateSut(string? condition = null, string? action = null, int? gotoQuestionId = null, int? gotoResultId = null) =>
        new(condition, action, gotoQuestionId, gotoResultId);
}