using System.Text.Json;
using TriageEngine.Models;

namespace TriageEngine;

public class TriageEngine : ITriageEngine
{
    public TriageState GetInitialState(Triage triage, string? savedStateJson = null)
    {
        if (string.IsNullOrEmpty(savedStateJson))
        {
            var firstQuestion = triage.Questions.Single(x => x.Id == triage.FirstQuestionId);
            return new TriageState(firstQuestion);
        }

        var state = JsonSerializer.Deserialize<EngineState>(savedStateJson);
        var question = triage.Questions.SingleOrDefault(x => x.Id == state?.QuestionId)
                       ?? triage.Questions.Single(x => x.Id == triage.FirstQuestionId);
        var result = triage.Results.SingleOrDefault(x => x.Id == state?.ResultId);

        return new TriageState(question, null, result);
    }

    public TriageState ProcessAnswer(string answer, TriageState currentState, Triage triage)
    {
        if (!ValidateAnswer(answer, currentState.CurrentQuestion))
        {
            throw new InvalidOperationException("Invalid answer.");
        }

        Question? nextQuestion = null;
        Result? result = null;

        foreach (var rule in GetRules(answer, currentState.CurrentQuestion))
        {
            if (!string.IsNullOrEmpty(rule.ActionString))
            {
                var action = ParseAction(rule.ActionString);
                action?.Invoke();
            }

            if (rule.GotoQuestionId is not null)
            {
                nextQuestion = triage.Questions.SingleOrDefault(x => x.Id == rule.GotoQuestionId);
                return new TriageState(currentState.CurrentQuestion, nextQuestion, null);
            }

            if (rule.GotoResultId is not null)
            {
                result = triage.Results.SingleOrDefault(x => x.Id == rule.GotoResultId);
            }
        }

        return new TriageState(currentState.CurrentQuestion, nextQuestion, result);

    }

    private static IEnumerable<Rule> GetRules(string answer, Question currentQuestion)
    {
        var rulesWithCondition = currentQuestion.Type switch
        {
            QuestionType.Text => currentQuestion.Rules?
                .Where(x => x.Condition != null && Rule.EvaluateAnswer(x.Condition, answer)),
            QuestionType.SingleChoice => currentQuestion.Rules?
                .Where(x => x.Condition != null && Rule.EvaluateAnswer(x.Condition, int.Parse(answer))),
            QuestionType.MultipleChoice => currentQuestion.Rules?
                .Where(x => x.Condition != null && Rule.EvaluateAnswer(x.Condition, answer.Split(',').Select(int.Parse))),
            QuestionType.FileUpload => throw new NotImplementedException(),
            _ => throw new NotSupportedException($"Question type {currentQuestion.Type} is not supported.")
        };

        if (rulesWithCondition?.Any() is true)
        {
            return rulesWithCondition;
        }

        var defaultRules = currentQuestion.Rules?
            .Where(x => x.Condition == null)
            .ToList();

        return defaultRules ?? throw new InvalidOperationException("No valid rule found.");
    }

    private static bool ValidateAnswer(string answer, Question currentQuestion) =>
        currentQuestion.Type switch
        {
            QuestionType.Text => !string.IsNullOrWhiteSpace(answer),
            QuestionType.SingleChoice => int.TryParse(answer, out _),
            QuestionType.MultipleChoice => answer.Split(',').All(x => int.TryParse(x, out _)),
            QuestionType.FileUpload => throw new NotImplementedException(),
            _ => throw new NotSupportedException($"Question type {currentQuestion.Type} is not supported.")
        };

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