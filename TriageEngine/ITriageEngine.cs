using TriageEngine.Models;

namespace TriageEngine;

public interface ITriageEngine
{
    TriageState GetInitialState(Triage triage, string? savedStateJson = null);
    TriageState ProcessAnswer(string answer, TriageState currentState, Triage triage);
}