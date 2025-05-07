namespace TriageEngine.Models;

public record TriageState
{
    public Question CurrentQuestion { get; }
    public Question? NextQuestion { get; }
    public Result? Result { get; }
    public bool IsComplete => Result != null;

    public TriageState(Question currentQuestion, Question? nextQuestion = null, Result? result = null)
    {
        CurrentQuestion = currentQuestion;
        NextQuestion = nextQuestion;
        Result = result;
    }

    public void Deconstruct(out Question currentQuestion, out Question? nextQuestion, out Result? result)
    {
        currentQuestion = CurrentQuestion;
        nextQuestion = NextQuestion;
        result = Result;
    }
}