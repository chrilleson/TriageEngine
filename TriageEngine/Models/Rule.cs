using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace TriageEngine.Models;

public record Rule
{
    public string? Condition { get; init; }
    public string? ActionString { get; init; }
    public int? GotoQuestionId { get; init; }
    public int? GotoResultId { get; init; }

    [JsonIgnore]
    public Action? Action => ParseAction(ActionString);

    public Rule(string? condition, string? actionString, int? gotoQuestionId, int? gotoResultId)
    {
        Condition = condition;
        ActionString = actionString;
        GotoQuestionId = gotoQuestionId;
        GotoResultId = gotoResultId;
    }

    public static bool EvaluateAnswer(string condition, int answer)
    {
        var compiledCondition = CompileCondition(condition);
        return compiledCondition(answer);
    }

    public void ExecuteAction() => Action?.Invoke();

    private static Action? ParseAction(string? action)
    {
        if (string.IsNullOrEmpty(action)) return null;

        var parts = action.Split(':', 2);
        if (!Enum.TryParse<ActionTypes>(parts[0], true, out var actionType))
        {
            return null;
        }

        var actionValue = parts.Length > 1 ? parts[1] : null;

        return actionType switch
        {
            ActionTypes.LogInformation => () => Console.WriteLine(actionValue),
            _ => null
        };
    }

    private static Func<int, bool> CompileCondition(string condition)
    {
        var parameter = Expression.Parameter(typeof(int), "x");

        var expression = System.Linq.Dynamic.Core.DynamicExpressionParser.ParseLambda(
            [parameter],
            typeof(bool),
            condition
        );

        return (Func<int, bool>)expression.Compile();
    }

    public void Deconstruct(out string? condition, out Action? action, out int? gotoQuestionId, out int? gotoResultId)
    {
        condition = Condition;
        action = Action;
        gotoQuestionId = GotoQuestionId;
        gotoResultId = GotoResultId;
    }
};