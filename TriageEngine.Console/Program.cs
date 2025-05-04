using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TriageEngine;
using TriageEngine.Models;

Console.OutputEncoding = Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<ITriageService, TriageService>();

using var host = builder.Build();

var triageService = host.Services.GetRequiredService<ITriageService>();

await Run(triageService);
return;

static async Task Run(ITriageService triageService)
{
    var menuItems = MenuItems();
    Console.WriteLine("Select a form to run:");
    foreach (var item in menuItems)
    {
        Console.WriteLine($"{item.Key}: {item.Value}");
    }

    if (!int.TryParse(Console.ReadLine(), out var selectedForm) || !menuItems.TryGetValue(selectedForm, out var formId))
    {
        Console.WriteLine("Invalid selection.");
        return;
    }

    Console.Clear();

    var triage = await triageService.ProcessTriageAsync(formId);

    Console.Clear();
    ProcessTriage(triage);
}

static IReadOnlyDictionary<int, string> MenuItems()
{
    var files = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Forms"), "*.json");
    var dictionary = new Dictionary<int, string>();
    foreach (var file in files)
    {
        dictionary.Add(dictionary.Count + 1, Path.GetFileNameWithoutExtension(file));
    }

    return dictionary;
}

static void ProcessTriage(Triage triage)
{
    var engineState = new EngineState(null, null);

    while (true)
    {
        var engine = TriageEngine.TriageEngine.Create(triage, JsonSerializer.Serialize(engineState));

        Console.Clear();
        Console.WriteLine($"Question: {engine.CurrentQuestion.Text}");
        if (engine is { CurrentQuestion.Options.Count: var count } && count != 0)
        {
            Console.WriteLine($"Options: {string.Join(", ", engine.CurrentQuestion.Options!.Select(x => $"{x.Key}: {x.Value}"))}");
        }

        Console.WriteLine("Answer: ");
        var answer = Console.ReadLine();
        if (string.IsNullOrEmpty(answer))
        {
            Console.WriteLine("Invalid answer.");
            continue;
        }

        engine.ProcessAnswer(answer);
        engineState = new EngineState(engine.NextQuestion?.Id, engine.Result?.Id);

        if (!engine.IsComplete) continue;

        Console.WriteLine($"Result: {engine.Result!.Text}");
        break;
    }
}