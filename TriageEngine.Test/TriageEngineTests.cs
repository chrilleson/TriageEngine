using Shouldly;
using TriageEngine.Models;

namespace TriageEngine.Test;

public class TriageEngineTests
{
    [Fact]
    public void ProcessTriage_ReturnsTriage()
    {
        var triageEngine = CreateSut();

        var result = triageEngine.ProcessTriage("Form");

        result.ShouldBeAssignableTo<Triage>();
        result.FormId.ShouldBe("TestForm");
        result.Questions.Count().ShouldBe(3);
    }

    private static TriageEngine CreateSut() =>
        new TriageEngine();
}