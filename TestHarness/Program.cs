using DSoft.ServiceRegistra;
using DSoft.ServiceRegistrar;
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

            ServiceRegistrar.Register<ITestInterface4, TestImplementation4>(obj =>
            {
                ((TestImplementation4)obj).Message = "Hello, init!";
            });

            //this should call the constructor action
            var serv4 = ServiceRegistrar.Service<ITestInterface4>();
            serv4.DoAThing();

            ServiceRegistrar.RegisterWithAutoDiscovery(typeof(TestImplementation2).Assembly);


            var imps = ServiceRegistrar.Instance.Services;

            var serv = ServiceRegistrar.Service<ITestInterface2>();

            var result = serv.TestInt();

            var serv2 = ServiceRegistrar.Service<TestInterface3>(new object[] { serv });


            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
