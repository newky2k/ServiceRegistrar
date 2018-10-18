using DSoft.ServiceRegistra;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestContracts
{
    public interface TestInterface3 : IAutoDiscoverableProvider
    {
        int TestInt();
    }
}
