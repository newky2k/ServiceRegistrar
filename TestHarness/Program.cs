using DSoft.ServiceRegistra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestContracts;
using TestImplementations;

namespace TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            //var assm = new List<Assembly>() { typeof(TestImplementation2).Assembly  };

            ServiceRegistra.RegisterWithAutoDiscovery(typeof(TestImplementation2).Assembly);

            var imps = ServiceRegistra.Instance.Services;

            var serv = ServiceRegistra.Service<ITestInterface2>();

            var result = serv.TestInt();

            var serv2 = ServiceRegistra.Service<TestInterface3>(new object[] { serv });


            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
