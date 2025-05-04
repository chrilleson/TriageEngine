using System.Text.Json;
using TriageEngine.Models;

namespace TriageEngine;

public class TriageEngine
{
    public Question? NextQuestion => _nextQuestion;
    public Result Result => _result!;
    public bool IsComplete => _result != null;

    private readonly Triage _triage;
    private readonly Question _currentQuestion;
    private Result? _result;
    private Question? _nextQuestion;
    private Action? _action;

    private TriageEngine(Triage triage, Question currentQuestion, Result? result)
    {
        _triage = triage;
        _currentQuestion = currentQuestion;
        _result = result;
    }

    public static TriageEngine Create(Triage triage, string engineState)
    {
        var state = JsonSerializer.Deserialize<EngineState>(engineState);
        var question = triage.Questions.SingleOrDefault(x => x.Id == state?.QuestionId) ?? triage.Questions.Single(x => x.Id == triage.FirstQuestionId);
        var result = triage.Results.SingleOrDefault(x => x.Id == state?.ResultId) ?? null;

        return new TriageEngine(triage, question, result);
    }

    public void ProcessAnswer(string answer)
    {
        var rule = _currentQuestion.Rules?.FirstOrDefault(x => x.Condition != null && Rule.EvaluateAnswer(x.Condition, int.Parse(answer))) ?? throw new InvalidOperationException("No valid rule found.");
        if (!string.IsNullOrEmpty(rule.ActionString))
        {
            _action = ParseAction(rule.ActionString);
            _action?.Invoke();
        }

        if (rule.GotoQuestionId is not null)
        {
            _nextQuestion = _triage.Questions.SingleOrDefault(x => x.Id == rule.GotoQuestionId);
            return;
        }

        if (rule.GotoResultId is not null)
        {
            _result = _triage.Results.SingleOrDefault(x => x.Id == rule.GotoResultId);
        }
    }

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
}