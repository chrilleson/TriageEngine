using System.Text.Json;
using TriageEngine.Actions.Factory;
using TriageEngine.Models;

namespace TriageEngine;

public class TriageEngine : ITriageEngine
{
    private readonly IActionFactory _actionFactory;

    public TriageEngine(IActionFactory actionFactory)
    {
        _actionFactory = actionFactory;
    }

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
                _actionFactory.Create(rule.ActionString)?.Execute();
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
}