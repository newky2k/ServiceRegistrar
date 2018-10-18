using System;
using System.Collections.Generic;
using System.Text;
using TestContracts;

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
