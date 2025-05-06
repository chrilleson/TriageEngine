using System.Text.Json;
using TriageEngine.Models;

namespace TriageEngine;

public class TriageEngine
{
    public Question CurrentQuestion { get; }

    public Question? NextQuestion { get; private set; }

    public Result Result => _result!;
    public bool IsComplete => _result != null;

    private readonly Triage _triage;
    private Result? _result;

    private TriageEngine(Triage triage, Question currentQuestion, Result? result)
    {
        CurrentQuestion = currentQuestion;
        _triage = triage;
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
        if (!ValidateAnswer(answer))
        {
            throw new InvalidOperationException("Invalid answer.");
        }

        foreach (var rule in GetRules(answer))
        {
            if (!string.IsNullOrEmpty(rule.ActionString))
            {
                var action = ParseAction(rule.ActionString);
                action?.Invoke();
            }

            if (rule.GotoQuestionId is not null)
            {
                NextQuestion = _triage.Questions.SingleOrDefault(x => x.Id == rule.GotoQuestionId);
                return;
            }

            if (rule.GotoResultId is not null)
            {
                _result = _triage.Results.SingleOrDefault(x => x.Id == rule.GotoResultId);
            }
        }
    }

    private IEnumerable<Rule> GetRules(string answer)
    {
        var rulesWithCondition = CurrentQuestion.Type switch
        {
            QuestionType.Text => CurrentQuestion.Rules?
                .Where(x => x.Condition != null && Rule.EvaluateAnswer(x.Condition, answer)),
            QuestionType.SingleChoice => CurrentQuestion.Rules?
                .Where(x => x.Condition != null && Rule.EvaluateAnswer(x.Condition, int.Parse(answer))),
            QuestionType.MultipleChoice => CurrentQuestion.Rules?
                .Where(x => x.Condition != null && Rule.EvaluateAnswer(x.Condition, answer.Split(',').Select(int.Parse))),
            QuestionType.FileUpload => throw new NotImplementedException(),
            _ => throw new NotSupportedException($"Question type {CurrentQuestion.Type} is not supported.")
        };

        if (rulesWithCondition?.Any() is true)
        {
            return rulesWithCondition;
        }

        var defaultRules = CurrentQuestion.Rules?
            .Where(x => x.Condition == null)
            .ToList();

        return defaultRules ?? throw new InvalidOperationException("No valid rule found.");
    }

    private bool ValidateAnswer(string answer)
    {
        return CurrentQuestion.Type switch
        {
            QuestionType.Text => !string.IsNullOrWhiteSpace(answer),
            QuestionType.SingleChoice => int.TryParse(answer, out _),
            QuestionType.MultipleChoice => answer.Split(',').All(x => int.TryParse(x, out _)),
            QuestionType.FileUpload => throw new NotImplementedException(),
            _ => throw new NotSupportedException($"Question type {CurrentQuestion.Type} is not supported.")
        };
    }

    private static Action? ParseAction(string action)
    {
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