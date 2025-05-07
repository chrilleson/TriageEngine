using Microsoft.Extensions.Logging;

namespace TriageEngine.Actions;

public class LogAction : IAction
{
    private readonly string _message;
    private readonly ILogger<LogAction> _logger;

    public LogAction(string message, ILogger<LogAction> logger)
    {
        _message = message;
        _logger = logger;
    }

    public void Execute()
    {
        _logger.LogInformation(_message);
    }
}