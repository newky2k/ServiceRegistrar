using DSoft.ServiceRegistrar;
using System;
using System.Collections.Generic;
using System.Text;
using TestContracts;
using TestImplementations;

[assembly: DiscoverableServiceAttribute(typeof(ITestInterface2), typeof(TestImplementation2))]
namespace TestImplementations
{
    public class TestImplementation2 : ITestInterface2
    {
        public int TestInt()
        {
            return 2;
        }
    }
}
