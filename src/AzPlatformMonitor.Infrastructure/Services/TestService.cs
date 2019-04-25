using AzPlatformMonitor.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzPlatformMonitor.Infrastructure.Services
{
    public class TestService : ITestService
    {
        public void IDoNothing()
        {
            throw new NotImplementedException();
        }
    }
}
