using DSoft.ServiceRegistrar;
using System;
using TestContracts;
using TestImplementations;

[assembly: DiscoverableServiceAttribute(typeof(ITestInterface1), typeof(TestImplementation1))]
namespace TestImplementations
{
    public class TestImplementation1 : ITestInterface1
    {
        public int TestInt()
        {
            return 1;
        }
    }
}
