using DdhpCore.FrontEnd;
using Microsoft.AspNetCore.Hosting;

namespace DdhpCore.Tests.Integration.Micros.Clubs
{
    public class TestStartup : Startup
    {
        public TestStartup(IHostingEnvironment env) : base(env)
        {
        }
    }
}