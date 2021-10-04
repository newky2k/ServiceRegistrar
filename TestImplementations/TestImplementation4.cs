using DSoft.ServiceRegistrar;
using System;
using System.Collections.Generic;
using System.Text;
using TestContracts;
using TestImplementations;

namespace TestImplementations
{
    
    public class TestImplementation4 : ITestInterface4
    {
        public string Message { get; set; }

        public void DoAThing()
        {
            Console.WriteLine(Message);
        }
    }
}
