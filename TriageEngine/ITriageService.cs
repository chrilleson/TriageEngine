using TriageEngine.Models;

namespace TriageEngine;

public interface ITriageService
{
    Triage ProcessTriage(string formId);
}