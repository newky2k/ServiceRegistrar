using DSoft.ServiceRegistrar;
using System;
using System.Collections.Generic;
using System.Text;
using TestContracts;
using TestImplementations;

[assembly: DiscoverableServiceAttribute(typeof(TestInterface3), typeof(TestImplementation3))]
namespace TestImplementations
{
    public class TestImplementation3 : TestInterface3
    {
        public TestImplementation3(object item)
        {
            var tr = item;
        }

        public int TestInt()
        {
            throw new NotImplementedException();
        }
    }
}
