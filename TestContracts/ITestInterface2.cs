using DSoft.ServiceRegistra;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestContracts
{
    public interface ITestInterface2 : IAutoDiscoverableProvider
    {
        int TestInt();
    }
}
