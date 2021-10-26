using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.AspNetCore;

namespace KOMTracker.API
{
    public class Program
    {
        public static int Main(string[] args) => CommonProgram.Main<Startup>(args);

        public static IHostBuilder CreateHostBuilder(string[] args) => CommonProgram.CreateHostBuilder<Startup>(args);
    }
}
