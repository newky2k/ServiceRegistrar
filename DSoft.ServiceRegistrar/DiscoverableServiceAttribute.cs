using System;
using System.Collections.Generic;
using System.Text;

namespace DSoft.ServiceRegistrar
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class DiscoverableServiceAttribute : Attribute
    {

        public DiscoverableServiceAttribute(Type interfaceType, Type implementationType)
        {
            Interface = interfaceType;
            Implementation = implementationType;
        }

        public Type Interface { get; private set; }

        public Type Implementation { get; private set; }
    }

}
