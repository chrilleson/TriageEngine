using System.Text.Json;
using TriageEngine.Actions;
using TriageEngine.Actions.Factory;
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
        var (sut, _) = CreateSut();

        var result = sut.GetInitialState(triage, engineState);

        result.ShouldBeAssignableTo<TriageState>();
        result.IsComplete.ShouldBeFalse();
        result.NextQuestion.ShouldBeNull();
    }

    [Fact]
    public void CreateEngine_EngineStateHasResultId_ReturnsEngineCompleted()
    {
        var questions = CreateQuestions();
        var results = CreateResults();
        var triage = CreateTriage(questions: questions, results: results);
        var engineState = JsonSerializer.Serialize(new EngineState(null, 1));
        var (sut, _) = CreateSut();

        var result = sut.GetInitialState(triage, engineState);

        result.ShouldBeAssignableTo<TriageState>();
        result.IsComplete.ShouldBeTrue();
        result.NextQuestion.ShouldBeNull();
    }

    [Fact]
    public void CreateEngine_EngineStateHasInvalidResultId_ResultIsNull()
    {
        var questions = CreateQuestions();
        var results = CreateResults();
        var triage = CreateTriage(questions: questions, results: results);
        var engineState = JsonSerializer.Serialize(new EngineState(null, 2));
        var (sut, _) = CreateSut();

        var result = sut.GetInitialState(triage, engineState);

        result.ShouldBeAssignableTo<TriageState>();
        result.IsComplete.ShouldBeFalse();
        result.NextQuestion.ShouldBeNull();
    }

    [Fact]
    public void ProcessAnswer_ValidAnswer_ReturnsNextQuestion()
    {
        var questions = CreateQuestions();
        var results = CreateResults();
        var triage = CreateTriage(questions: questions, results: results);
        var triageState = new TriageState(triage.Questions.First());
        var (sut, actionFactory) = CreateSut();

        var result = sut.ProcessAnswer("1", triageState, triage);

        result.NextQuestion.ShouldNotBeNull();
        result.NextQuestion.Id.ShouldBe(2);
        result.NextQuestion.Text.ShouldBe("Question 2");
        result.IsComplete.ShouldBeFalse();
        actionFactory.Received(1).Create($"{ActionTypes.LogInformation.ToString()}:Adult");
    }

    [Fact]
    public void ProcessAnswer_ValidAnswer_ReturnsResult()   
    {
        var questions = CreateQuestions();
        var results = CreateResults();
        var triage = CreateTriage(questions: questions, results: results);
        var triageState = new TriageState(triage.Questions.Single(x => x.Id == 2));
        var (sut, actionFactory) = CreateSut();

        var result = sut.ProcessAnswer("1", triageState, triage);

        result.NextQuestion.ShouldBeNull();
        result.IsComplete.ShouldBeTrue();
        result.Result.ShouldNotBeNull();
        result.Result.Id.ShouldBe(1);
        result.Result.Text.ShouldBe("Result 1");
        actionFactory.Received(1).Create($"{ActionTypes.LogInformation.ToString()}:Option1");
    }

    private static (TriageEngine, IActionFactory) CreateSut()
    {
        var actionFactroy = Substitute.For<IActionFactory>();
        var sut = new TriageEngine(actionFactroy);

        return (sut, actionFactroy);
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
            "Are you older than 18?",
            QuestionType.SingleChoice,
            new Dictionary<int, string> {{ 1, "Yes" }, { 2, "No" }},
            [
                new Rule("x == 1", $"{ActionTypes.LogInformation.ToString()}:Adult", 2, null),
                new Rule("x == 2", $"{ActionTypes.LogInformation.ToString()}:Child", null, 1)
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