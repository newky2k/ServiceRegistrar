using System;
using System.Collections.Generic;
using System.Text;

namespace DSoft.ServiceRegistrar
{
    /// <summary>
    /// Discoverable Service Attribute.
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class DiscoverableServiceAttribute : Attribute
    {
        /// <summary>
        /// Gets the interface.
        /// </summary>
        /// <value>The interface.</value>
        public Type Interface { get; private set; }

        /// <summary>
        /// Gets the implementation.
        /// </summary>
        /// <value>The implementation.</value>
        public Type Implementation { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoverableServiceAttribute"/> class.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        public DiscoverableServiceAttribute(Type interfaceType, Type implementationType)
        {
            Interface = interfaceType;
            Implementation = implementationType;
        }


    }

}
