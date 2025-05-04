using System.Text.Json;
using TriageEngine.Models;

namespace TriageEngine;

public class TriageService : ITriageService
{
    public Triage ProcessTriage(string formId)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Forms", $"{formId}.json");
        var json = File.ReadAllText(file);
        var triage = JsonSerializer.Deserialize<Triage>(json) ?? throw new InvalidOperationException("Failed to deserialize Triage object.");

        return triage;
    }

    public async Task<Triage> ProcessTriageAsync(string formId)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Forms", $"{formId}.json");
        await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
        var triage = await JsonSerializer.DeserializeAsync<Triage>(stream) ?? throw new InvalidOperationException("Failed to deserialize Triage object.");

        return triage;
    }
}