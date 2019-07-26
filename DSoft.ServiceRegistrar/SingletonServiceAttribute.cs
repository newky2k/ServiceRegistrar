using System;
using System.Collections.Generic;
using System.Text;

namespace DSoft.ServiceRegistrar
{

    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonServiceAttribute : Attribute
    {

    }
}
