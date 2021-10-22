using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.DependencyInjection;

namespace Utils.Tests.Logging
{
    public interface ITestLoggerFactory
    {
        ITestLogger Create();

        ITestLogger<T> Create<T>();
    }

    public class TestLoggerFactory : ITestLoggerFactory
    {
        private readonly ITestOutputHelperAccessor _outputHelperAccessor;

        public TestLoggerFactory(ITestOutputHelperAccessor outputHelperAccessor)
        {
            _outputHelperAccessor = outputHelperAccessor ?? throw new ArgumentNullException(nameof(outputHelperAccessor));
        }

        public ITestLogger Create() => new TestLogger(_outputHelperAccessor);

        public ITestLogger<T> Create<T>() => new TestLogger<T>(_outputHelperAccessor);
    }
}
