using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ShopfloorAssistant.Core
{
    public static class LoggerExtensions
    {
        public static IDisposable LogElapsed(this ILogger logger, string message)
        {
            return new TimedLogger(logger, message);
        }

        private class TimedLogger : IDisposable
        {
            private readonly ILogger _logger;
            private readonly string _message;
            private readonly Stopwatch _sw;

            public TimedLogger(ILogger logger, string message)
            {
                _logger = logger;
                _message = message;
                _sw = Stopwatch.StartNew();
                _logger.LogInformation("{Message} started", _message);
            }

            public void Dispose()
            {
                _sw.Stop();
                _logger.LogInformation("{Message} finished in {ElapsedSeconds:F3} seconds",
                                _message,
                                _sw.Elapsed.TotalSeconds);
            }
        }
    }

}
