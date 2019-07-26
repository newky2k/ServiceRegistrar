using DSoft.ServiceRegistrar;
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
