using DSoft.ServiceRegistrar;
using TestContracts;
using TestImplementations;

namespace TestHarnessTwo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Services.Register<ITestInterface4, TestImplementation4>(obj =>
            {
                obj.Message = "Hello, init!";
            });

            //this should call the constructor action
            var serv4 = Services.Get<ITestInterface4>();

            serv4.DoAThing();

            Services.Register(typeof(TestImplementation2).Assembly);

            var serv = Services.Get<ITestInterface2>();

            var result = serv.TestInt();

            var serv2 = Services.Get<TestInterface3>(new object[] { serv });

            Services.Register<TestImplementation5>();

            var serv5 = Services.Get<TestImplementation5>();

            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
