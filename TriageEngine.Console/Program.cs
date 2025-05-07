using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using TriageEngine;
using TriageEngine.Models;

Console.OutputEncoding = Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
    .CreateLogger();

builder.Logging.AddSerilog();

builder.Services.AddSerilog(dispose: true);
builder.Services.AddTriageEngine();

using var host = builder.Build();

await Run(host.Services);
return;

static async Task Run(IServiceProvider serviceProvider)
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

    var triageService = serviceProvider.GetRequiredService<ITriageService>();

    var triage = await triageService.ProcessTriageAsync(formId);
    var triageEngine = serviceProvider.GetRequiredService<ITriageEngine>();

    Console.Clear();
    ProcessTriage(triage, triageEngine);
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

static void ProcessTriage(Triage triage, ITriageEngine triageEngine)
{
    var engineState = new EngineState(null, null);

    while (true)
    {
        var triageState = triageEngine.GetInitialState(triage, JsonSerializer.Serialize(engineState));

        Console.Clear();
        Console.WriteLine($"Question: {triageState.CurrentQuestion.Text}");
        if (triageState is { CurrentQuestion.Options.Count: var count } && count != 0)
        {
            Console.WriteLine($"Options: {string.Join(", ", triageState.CurrentQuestion.Options!.Select(x => $"{x.Key}: {x.Value}"))}");
        }

        Console.WriteLine("Answer: ");
        var answer = Console.ReadLine();
        if (string.IsNullOrEmpty(answer))
        {
            Console.WriteLine("Invalid answer.");
            continue;
        }

        triageState = triageEngine.ProcessAnswer(answer, triageState, triage);
        engineState = new EngineState(triageState.NextQuestion?.Id, triageState.Result?.Id);

        if (!triageState.IsComplete) continue;

        Console.WriteLine($"Result: {triageState.Result!.Text}");
        break;
    }
}