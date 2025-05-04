using TriageEngine.Models;

namespace TriageEngine.Test;

public class TriageServiceTests
{
    [Fact]
    public void ProcessTriage_ReturnsTriage()
    {
        var triageService = CreateSut();

        var result = triageService.ProcessTriage("Form");

        result.ShouldBeAssignableTo<Triage>();
        result.FormId.ShouldBe("TestForm");
        result.Questions.Count().ShouldBe(3);
    }

    private static TriageService CreateSut() => new();
}