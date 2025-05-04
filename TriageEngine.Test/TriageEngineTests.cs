using System.Text.Json;
using TriageEngine.Models;

namespace TriageEngine.Test;

public class TriageEngineTests
{
    [Fact]
    public void CreateEngine_EngineStateIsEmpty_ReturnsEngineInDefaultState()
    {
        var questions = CreateQuestions();
        var results = CreateResults();
        var triage = CreateTriage(questions: questions, results: results);
        var engineState = JsonSerializer.Serialize(new EngineState(null, null));

        var engine = TriageEngine.Create(triage, engineState);

        engine.ShouldBeAssignableTo<TriageEngine>();
        engine.IsComplete.ShouldBeFalse();
        engine.NextQuestion.ShouldBeNull();
    }

    [Fact]
    public void CreateEngine_EngineStateHasResultId_ReturnsEngineCompleted()
    {
        var questions = CreateQuestions();
        var results = CreateResults();
        var triage = CreateTriage(questions: questions, results: results);
        var engineState = JsonSerializer.Serialize(new EngineState(null, 1));

        var engine = TriageEngine.Create(triage, engineState);

        engine.ShouldBeAssignableTo<TriageEngine>();
        engine.IsComplete.ShouldBeTrue();
        engine.NextQuestion.ShouldBeNull();
    }

    [Fact]
    public void CreateEngine_EngineStateHasInvalidResultId_ResultIsNull()
    {
        var questions = CreateQuestions();
        var results = CreateResults();
        var triage = CreateTriage(questions: questions, results: results);
        var engineState = JsonSerializer.Serialize(new EngineState(null, 2));

        var engine = TriageEngine.Create(triage, engineState);

        engine.ShouldBeAssignableTo<TriageEngine>();
        engine.IsComplete.ShouldBeFalse();
        engine.NextQuestion.ShouldBeNull();
    }

    [Fact]
    public void ProcessAnswer_ValidAnswer_ReturnsNextQuestion()
    {
        var questions = CreateQuestions();
        var results = CreateResults();
        var triage = CreateTriage(questions: questions, results: results);
        var engineState = JsonSerializer.Serialize(new EngineState(null, null));
        var engine = TriageEngine.Create(triage, engineState);

        engine.ProcessAnswer("20");

        engine.NextQuestion.ShouldNotBeNull();
        engine.NextQuestion.Id.ShouldBe(2);
        engine.NextQuestion.Text.ShouldBe("Question 2");
        engine.IsComplete.ShouldBeFalse();
    }

    [Fact]
    public void ProcessAnswer_ValidAnswer_ReturnsResult()
    {
        var questions = CreateQuestions();
        var results = CreateResults();
        var triage = CreateTriage(questions: questions, results: results);
        var engineState = JsonSerializer.Serialize(new EngineState(2, null));
        var engine = TriageEngine.Create(triage, engineState);

        engine.ProcessAnswer("1");

        engine.NextQuestion.ShouldBeNull();
        engine.IsComplete.ShouldBeTrue();
        engine.Result.ShouldNotBeNull();
        engine.Result.Id.ShouldBe(1);
        engine.Result.Text.ShouldBe("Result 1");
    }

    private static Triage CreateTriage(string formId = "TestForm", int firstQuestionId = 1, IEnumerable<Question>? questions = null, IEnumerable<Result>? results = null) =>
        new(
            FormId: formId,
            FirstQuestionId: firstQuestionId,
            Questions: questions ?? [],
            Results: results ?? []
        );

    private static IEnumerable<Question> CreateQuestions() =>
    [
        new(
            1,
            "What is your age?",
            QuestionType.Text,
            null,
            [
                new Rule("x > 18", $"{ActionTypes.LogInformation.ToString()}:Adult", 2, null),
                new Rule("x < 18", $"{ActionTypes.LogInformation.ToString()}:Child", null, 1)
            ]
        ),
        new(
            2,
            "Question 2",
            QuestionType.SingleChoice,
            new Dictionary<int, string> {{ 1, "Option 1" }, { 2, "Option 2" }},
            [
                new Rule("x == 1", $"{ActionTypes.LogInformation.ToString()}:Option1", null, 1),
                new Rule("x == 2", $"{ActionTypes.LogInformation.ToString()}:Option2", 1, null)
            ]
        )
    ];

    private static IEnumerable<Result> CreateResults() =>
    [
        new(1, "Result 1"),
    ];
}