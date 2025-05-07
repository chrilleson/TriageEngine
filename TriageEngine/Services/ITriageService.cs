using TriageEngine.Models;

namespace TriageEngine.Services;

public interface ITriageService
{
    Triage ProcessTriage(string formId);
    Task<Triage> ProcessTriageAsync(string formId);
}