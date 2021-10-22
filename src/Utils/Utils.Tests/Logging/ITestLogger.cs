using Microsoft.Extensions.Logging;

namespace Utils.Tests.Logging
{
    public interface ITestLogger : ILogger
    {
        internal IMockLogger MockLoggerSubstitute { get; }
    }
    public interface ITestLogger<T> : ITestLogger, ILogger<T> { }
}
