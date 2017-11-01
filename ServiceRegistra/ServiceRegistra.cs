using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace ServiceRegistra
{
    /// <summary>
    /// Dependency Injection and Service Management Implementation
    /// </summary>
    public class ServiceRegistra
    {
        #region Fields

        private Dictionary<Type, Type> services;

        private static Lazy<ServiceRegistra> instance = new Lazy<ServiceRegistra>(() => new ServiceRegistra());

        #endregion

        #region Properties


        /// <summary>
        /// Shared instance of ServiceRegistra
        /// </summary>
        public static ServiceRegistra Instance
        {
            get
            {
                return instance.Value;
            }
        }

        public Dictionary<Type, Type> Services
        {
            get
            {
                return services;
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ServiceRegistra.ServiceRegistra"/> class.
        /// </summary>
        private ServiceRegistra()
        {
            services = new Dictionary<Type, Type>();
        }

        #endregion

        #region Methods

       
        /// <summary>
        /// Register a service and its corresponding implementation
        /// </summary>
        /// <typeparam name="T">The service interface</typeparam>
        /// <typeparam name="T2">The service implementation, must implement or be a subclass of the service definition</typeparam>
        public static void Register<T, T2>() where T2 : T
        {
            Register(typeof(T), typeof(T2));

        }

        /// <summary>
        /// Register a service and its corresponding implementation
        /// </summary>
        /// <returns>The register.</returns>
        /// <param name="sInterface">The service interface type</param>
        /// <param name="sImplementation">The service implementation</param>
        public static void Register(Type sInterface, Type sImplementation)
        {
            if (!sInterface.GetTypeInfo().IsInterface)
                throw new ArgumentException(String.Format("You cannot register {0} as a service interface in ServiceRegistra as it is not an interface", sInterface.FullName));

            if (Instance.services.ContainsKey(sInterface))
                Instance.services[sInterface] = sImplementation;
            else
                Instance.services.Add(sInterface, sImplementation);
        }


        /// <summary>
        /// Register service interfaces and implementations from the assemblies with the registra
        /// </summary>
        /// <typeparam name="T">A type from the assembly with the service interfaces</typeparam>
        /// <typeparam name="T2">A type from the assembly with the service implmentations</typeparam>
        public static void RegisterFromAssemblies<T, T2>()
        {
            //find all the interfaces that implement the base interface first
            var siAssembly = typeof(T).GetTypeInfo().Assembly;
            var impAssembly = typeof(T2).GetTypeInfo().Assembly;

            RegisterFromAssemblies(siAssembly, impAssembly);
        }

        /// <summary>
        /// Register service interfaces and implementations from the assemblies with the registra
        /// </summary>
        /// <param name="interfaces">The assembly with the service interfaces</param>
        /// <param name="implementations">The assembly with the service implmentations</param>
        public static void RegisterFromAssemblies(Assembly interfaces, Assembly implementations)
        {
            var servInterfaces = interfaces.GetTypes().Where(x => x.GetTypeInfo().IsInterface).ToList();


            foreach (var siType in servInterfaces)
            {
                //then find all of the first implementation in the provided assembly
                var impltype = implementations.GetTypes().FirstOrDefault(x => siType.GetTypeInfo().IsAssignableFrom(x));

                if (impltype != null)
                {
                    //yay!, register
                    Register(siType, impltype);
                }

            }
        }

        /// <summary>
        /// Find the implementing service for the speficied interface
        /// </summary>
        /// <typeparam name="T">The service interface</typeparam>
        /// <returns>The implementation of the service interface</returns>
        public static T Service<T>()
        {
            var typ = typeof(T);

            if (!Instance.services.ContainsKey(typ))
            {
                throw new Exception(string.Format("There is no registered implementation for type: {0}", typ.FullName));
            }

            var imp = Instance.services[typ];

            var conts = imp.GetTypeInfo().GetConstructors();

            var cPars = new List<object>();

            if (conts.Length > 0)
            {
                var ctr = conts[0];

                var pars = ctr.GetParameters();

                if (pars.Length != 0)
                {
                    foreach (var aPar in pars)
                    {
                        var pType = aPar.ParameterType;

                        var pImp = Service(pType);

                        if (pImp != null)
                            cPars.Add(pImp);
                    }
                }

            }

            var inst = (cPars.Count == 0) ? (T)Activator.CreateInstance(imp) : (T)Activator.CreateInstance(imp, cPars.ToArray());

            return inst;

        }

        /// <summary>
        /// Find the implementing service for the speficied interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object Service(Type type)
        {
            var typ = type;

            if (!Instance.services.ContainsKey(typ))
            {
                return null;
            }

            var imp = Instance.services[typ];

            var conts = imp.GetTypeInfo().GetConstructors();

            var cPars = new List<object>();

            if (conts.Length > 0)
            {
                var ctr = conts[0];

                var pars = ctr.GetParameters();

                if (pars.Length != 0)
                {
                    foreach (var aPar in pars)
                    {
                        var pType = aPar.GetType();

                        var pImp = Service(pType);

                        if (pImp != null)
                            cPars.Add(pImp);
                    }
                }

            }

            var inst = (cPars.Count == 0) ? Activator.CreateInstance(imp) : Activator.CreateInstance(imp, cPars.ToArray());

            return inst;

        }

        #endregion
    }
}
