using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using Xunit.Abstractions;

namespace Utils.Tests.Logging
{
    internal interface IMockLogger
    {
        void Log(LogLevel logLevel, string message);
    }

    /// <summary>
    /// All logs is done by:
    /// void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);
    /// 
    /// but TStat become internal https://github.com/dotnet/extensions/issues/1319
    /// So simplie check Recived().Log() cannot be done
    /// 
    /// This need to be proxied to method with accessible type etc.
    /// https://github.com/nsubstitute/NSubstitute/issues/597#issuecomment-653555567
    /// </summary>
    internal abstract class TestLoggerBase : ITestLogger
    {
        private readonly IMockLogger _mockLoggerSubstitute = Substitute.For<IMockLogger>();

        IMockLogger ITestLogger.MockLoggerSubstitute => _mockLoggerSubstitute;

        public TestLoggerBase(ITestOutputHelper outputHelper)
        {
            ConfigureOutput(outputHelper);
        }


        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => _mockLoggerSubstitute.Log(logLevel, formatter(state, exception));

        protected void ConfigureOutput(ITestOutputHelper outputHelper)
        {
            if (outputHelper != null)
            {
                _mockLoggerSubstitute.WhenForAnyArgs(x => x.Log(Arg.Any<LogLevel>(), Arg.Any<string>()))
                   .Do(x =>
                   {
                       var logLevel = x.Arg<LogLevel>();
                       var logMessage = x.Arg<string>();
                       outputHelper.WriteLine($"{logLevel}: {logMessage}");
                   });
            }
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
            => true;

        IDisposable ILogger.BeginScope<TState>(TState state)
            => throw new NotImplementedException();
    }
}
