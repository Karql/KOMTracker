using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Exceptions;
using System.Text.RegularExpressions;

namespace Utils.Tests.Logging
{
    public static class TestLoggerExtensions
    {
        public static void CheckLogDebug(this ITestLogger logger, string message, int? requiredNumberOfCalls = null)
            => logger.CheckLog(LogLevel.Debug, message, requiredNumberOfCalls);

        public static void CheckLogDebug(this ITestLogger logger, Regex pattern, int? requiredNumberOfCalls = null)
            =>logger.CheckLog(LogLevel.Debug, pattern, requiredNumberOfCalls);

        public static void CheckLogWarning(this ITestLogger logger, string message, int? requiredNumberOfCalls = null)
            => logger.CheckLog(LogLevel.Warning, message, requiredNumberOfCalls);

        public static void CheckLogWarning(this ITestLogger logger, Regex pattern, int? requiredNumberOfCalls = null)
            => logger.CheckLog(LogLevel.Warning, pattern, requiredNumberOfCalls);

        public static void CheckLogError(this ITestLogger logger, string message = null, int? requiredNumberOfCalls = null)
            => logger.CheckLog(LogLevel.Error, message, requiredNumberOfCalls);

        public static void CheckLogError(this ITestLogger logger, Regex pattern, int? requiredNumberOfCalls = null)
            => logger.CheckLog(LogLevel.Error, pattern, requiredNumberOfCalls);

        public static void CheckLog(this ITestLogger logger, LogLevel logLevel, string message = null, int? requiredNumberOfCalls = null)
        {
            var regex = !string.IsNullOrEmpty(message) ? new Regex(message) : null;
            logger.CheckLog(logLevel, regex, requiredNumberOfCalls);
        }
        public static void CheckLog(this ITestLogger logger, LogLevel logLevel, Regex pattern = null, int? requiredNumberOfCalls = null)
        {
            var substitute = logger.MockLoggerSubstitute;
            var recived = requiredNumberOfCalls.HasValue ? substitute.Received(requiredNumberOfCalls.Value) : substitute.Received();

            try
            {
                if (pattern != null)
                {
                    recived.Log(logLevel, Arg.Is<string>(x => pattern.IsMatch(x)));
                }
                else
                {
                    recived.Log(logLevel, Arg.Any<string>());
                }
            }
            catch (ReceivedCallsException ex)
            {
                throw new ReceivedCallsException($"Not received call with message matched pattern: {pattern}", ex);
            }
        }

    }
}
