using NSubstitute;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;
using Xunit.DependencyInjection;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]//To be visible by DI
namespace Utils.Tests.Logging;

internal class TestLogger : TestLoggerBase, ITestLogger
{
    public TestLogger(ITestOutputHelperAccessor outputHelperAccessor) : base(outputHelperAccessor.Output) { }
}

internal class TestLogger<T> : TestLogger, ITestLogger<T>
{
    public TestLogger(ITestOutputHelperAccessor outputHelperAccessor) : base(outputHelperAccessor) { }
}
