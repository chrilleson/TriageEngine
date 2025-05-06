using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace TriageEngine.Models;

public record Rule
{
    public string? Condition { get; init; }
    public string? ActionString { get; init; }
    public int? GotoQuestionId { get; init; }
    public int? GotoResultId { get; init; }

    public Rule(string? condition, string? actionString, int? gotoQuestionId, int? gotoResultId)
    {
        Condition = condition;
        ActionString = actionString;
        GotoQuestionId = gotoQuestionId;
        GotoResultId = gotoResultId;
    }

    public static bool EvaluateAnswer<T>(string condition, T answer)
    {
        var compiledCondition = CompileCondition<T>(condition);
        return compiledCondition(answer);
    }

    private static Func<TIn, bool> CompileCondition<TIn>(string condition)
    {
        var parameter = Expression.Parameter(typeof(TIn), "x");

        var expression = System.Linq.Dynamic.Core.DynamicExpressionParser.ParseLambda(
            [parameter],
            typeof(bool),
            condition
        );

        return (Func<TIn, bool>)expression.Compile();
    }

    public void Deconstruct(out string? condition, out string? actionstring, out int? gotoQuestionId, out int? gotoResultId)
    {
        condition = Condition;
        actionstring = ActionString;
        gotoQuestionId = GotoQuestionId;
        gotoResultId = GotoResultId;
    }
};