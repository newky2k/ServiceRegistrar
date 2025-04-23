using System;
using System.Collections.Generic;
using System.Text;

namespace DSoft.ServiceRegistrar
{

    /// <summary>
    /// Singleton Service Attribute.
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonServiceAttribute : Attribute
    {

    }
}
