using Microsoft.Extensions.Logging;

namespace Finance.Api.Extentions
{
    public static class LoggerExtensions
    {
        public static IDisposable TimedOperation<T>(
            this ILogger<T> logger, string message, params object?[] args)
        {
            return new TimedLogOperation<T>(logger, LogLevel.Information, message, args);
        }
    }

    // Useage
    //using var _ = _logger.TimedOperation("Some type of message");
}
