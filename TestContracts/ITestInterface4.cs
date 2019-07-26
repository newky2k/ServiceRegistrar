using DSoft.ServiceRegistrar;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestContracts
{
    public interface ITestInterface4 : IAutoDiscoverableProvider
    {
        void DoAThing();
    }
}
