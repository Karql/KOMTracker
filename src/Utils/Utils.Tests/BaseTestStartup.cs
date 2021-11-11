using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Tests.Logging;

namespace Utils.Tests;

public class BaseTestStartup
{
    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<ITestLoggerFactory, TestLoggerFactory>();
        services.AddTransient<ITestLogger, TestLogger>();
        services.AddTransient(typeof(ITestLogger<>), typeof(TestLogger<>));
    }
}
