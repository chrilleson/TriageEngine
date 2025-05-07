using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TriageEngine.Models;

namespace TriageEngine.Actions.Factory;

public class ActionFactory : IActionFactory
{
    private readonly Dictionary<string, Func<string, IServiceProvider, IAction>> _actionCreators = new();
    private readonly IServiceProvider _serviceProvider;

    public ActionFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        RegisterDefaultActions();
    }

    public IAction? Create(string? actionString)
    {
        if (string.IsNullOrEmpty(actionString))
            return null;

        var parts = actionString.Split(':', 2);
        if (parts.Length == 0)
            return null;

        var actionType = parts[0];
        var actionValue = parts.Length > 1 ? parts[1] : string.Empty;

        return _actionCreators.TryGetValue(actionType, out var creator)
            ? creator(actionValue, _serviceProvider)
            : null;
    }

    public void RegisterAction(string actionType, Func<string, IServiceProvider, IAction> creator)
    {
        _actionCreators[actionType] = creator;
    }

    private void RegisterDefaultActions()
    {
        RegisterAction(ActionTypes.LogInformation.ToString(),
            (message, sp) => new LogAction(message, sp.GetRequiredService<ILogger<LogAction>>()));
    }
}