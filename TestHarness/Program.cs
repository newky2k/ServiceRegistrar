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

            ServiceRegistrar.Register<ITestInterface4, TestImplementation4>(obj =>
            {
                obj.Message = "Hello, init!";
            });

            //this should call the constructor action
            var serv4 = ServiceRegistrar.Get<ITestInterface4>();

            serv4.DoAThing();

            ServiceRegistrar.Register(typeof(TestImplementation2).Assembly);

            var serv = ServiceRegistrar.Get<ITestInterface2>();

            var result = serv.TestInt();

            var serv2 = ServiceRegistrar.Get<TestInterface3>(new object[] { serv });

            ServiceRegistrar.Register<TestImplementation5>();

            var serv5 = ServiceRegistrar.Get<TestImplementation5>();

            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
