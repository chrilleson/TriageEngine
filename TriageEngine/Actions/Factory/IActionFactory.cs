namespace TriageEngine.Actions.Factory;

public interface IActionFactory
{
    IAction? Create(string? actionString);
    void RegisterAction(string actionType, Func<string, IServiceProvider, IAction> creator);
}