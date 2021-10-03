using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DSoft.ServiceRegistrar
{
    /// <summary>
    /// Dependency Injection and Service Management Container
    /// </summary>
    public class ServiceRegistrar
    {
        #region Fields

        private Dictionary<Type, Type> _services = new Dictionary<Type, Type>();
        private Dictionary<Type, object> _cachedServices = new Dictionary<Type, object>();
        private Dictionary<Type, Action<object>> _constructorActions=  new Dictionary<Type, Action<object>>();
        private List<Type> _singleTons = new List<Type>();
        protected static Lazy<ServiceRegistrar> _instance = new Lazy<ServiceRegistrar>(() => new ServiceRegistrar());

        #endregion

        #region Properties


        /// <summary>
        /// Shared instance of ServiceRegistra
        /// </summary>
        public static ServiceRegistrar Instance => _instance.Value;

        public Dictionary<Type, Type> Services => _services;

        public Dictionary<Type, object> CachedServices => _cachedServices;

        #endregion

        #region Methods

        #region Register

        /// <summary>
        /// Register a service and its corresponding implementation
        /// </summary>
        /// <typeparam name="T">The service interface</typeparam>
        /// <typeparam name="T2">The service implementation, must implement or be a subclass of the service definition</typeparam>
        /// <param name="constructorAction">The constructor action to call after the instance is created</param>
        public static void Register<T, T2>(Action<T2> constructorAction = null) where T2 : class, T
        {
            Action<object> executor = null;

            if (constructorAction != null)
                executor = new Action<object>((obj) => { constructorAction((T2)obj);});

            Register(typeof(T), typeof(T2), executor);
        }

        /// <summary>
        /// Registers a singleton instance of the implementation.  The constructor action will be called the first time the services is requested
        /// </summary>
        /// <typeparam name="T">The service interface</typeparam>
        /// <typeparam name="T2">The service implementation, must implement or be a subclass of the service definition</typeparam>
        /// <param name="constructorAction">The constructor action to call after the instance is created</param>
        public static void RegisterSingleTon<T, T2>(Action<T2> constructorAction = null) where T2 : class, T
        {
            Action<object> executor = null;

            if (constructorAction != null)
                executor = new Action<object>((obj) => { constructorAction((T2)obj); });

            Register(typeof(T), typeof(T2), executor);

            if (!Instance._singleTons.Contains(typeof(T)))
                Instance._singleTons.Add(typeof(T));
                
        }

        /// <summary>
        /// Register service interfaces and implementations from the assemblies with the registra
        /// </summary>
        /// <typeparam name="T">A type from the assembly with the service interfaces</typeparam>
        /// <typeparam name="T2">A type from the assembly with the service implmentations</typeparam>
        public static void RegisterFromAssemblies<T, T2>() => RegisterFromAssemblies(typeof(T).GetTypeInfo().Assembly, typeof(T2).GetTypeInfo().Assembly);

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
        /// Register the calling assembly as the implementation assembly
        /// </summary>
        /// <typeparam name="T">The interface to find implementations of</typeparam>
        public static void RegisterFromCallingAssembly<T>() => RegisterFromAssemblies(typeof(T).GetTypeInfo().Assembly, Assembly.GetCallingAssembly());

        /// <summary>
        /// Register the calling assembly as the implementation assembly
        /// </summary>
        /// <param name="interfaces">the interfaces assembly</param>
        public static void RegisterFromCallingAssembly(Assembly interfaceAssembly) => RegisterFromAssemblies(interfaceAssembly, Assembly.GetCallingAssembly());



        /// <summary>
        /// Register the assmebly and process referenced assemblie to auto-discover implementations of interfaces that inherit from IAutoDiscoverableProvider
        /// 
        /// Note: Linking may remove unused references so they will not loaded
        /// </summary>
        /// <param name="assm"></param>
        public static void RegisterWithAutoDiscovery(Assembly assm)
        {
            var getAsssmMeths = assm.GetType().GetTypeInfo().GetDeclaredMethods("GetReferencedAssemblies").ToList();

            if (getAsssmMeths == null && getAsssmMeths.Count == 0)
                throw new Exception("Unable to call GetReferencedAssessmblies on the Assembly object passed to ServiceRegistrar.RegisterWithAutoDiscovery");

            var methods = getAsssmMeths.Where(x => x.IsPublic.Equals(true)).ToList();

            if (methods.Count == 0)
                throw new Exception("Unable to call the Public method GetReferencedAssessmblies on the Assembly object passed to ServiceRegistrar.RegisterWithAutoDiscovery");

            var firMethd = methods.First();

            var assms = (AssemblyName[])firMethd.Invoke(assm, null);

            var newAssms = new List<AssemblyName>() { assm.GetName() };
            newAssms.AddRange(assms);

            var loadedAssms = LoadAssemblies(newAssms);

            RegisterWithAutoDiscovery(loadedAssms);
        }

        /// <summary>
        /// Registers the calling assembly and auto-discovers implementations of interfaces that inherit from IAutoDiscoverableProvider
        /// </summary>
        public static void RegisterWithAutoDiscovery() => RegisterWithAutoDiscovery(Assembly.GetCallingAssembly());

        /// <summary>
        /// Register the assmeblies array and auto-discovers implementations of interfaces that inherit from IAutoDiscoverableProvider
        /// </summary>
        /// <param name="assemblies">Array of assemblies to work through</param>
        public static void RegisterWithAutoDiscovery(Assembly[] assemblies)
        {

            var aUiprovider = typeof(IAutoDiscoverableProvider);

            foreach (var aItem in assemblies)
            {
                var dTypes = aItem.DefinedTypes;
                var atypes = dTypes.Where(x => aUiprovider.GetTypeInfo().IsAssignableFrom(x)).ToList();

                foreach (var aImp in atypes)
                {
                    if (!aImp.IsInterface)
                    {
                        var interfacs = aImp.ImplementedInterfaces.Where(x => !x.GetTypeInfo().Equals(aUiprovider) && aUiprovider.GetTypeInfo().IsAssignableFrom(x.GetTypeInfo())).ToList();

                        if (interfacs.Count > 0)
                        {

                            var firstInt = interfacs.FirstOrDefault();

                            if (firstInt != null)
                            {
                                Register(firstInt, aImp.AsType());
                            }

                        }
                    }

                }



            }
        }

        #endregion

        #region Service

        [Obsolete("Use Get<T>(object[]) instead")]
        /// <summary>
        /// Find the implementing service for the speficied interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initObjects">Constructor parameter objects</param>
        /// <returns></returns>
        public static T Service<T>(object[] initObjects = null) => Get<T>(initObjects);

        /// <summary>
        /// Find the implementing service for the speficied interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initObjects">Constructor parameter objects</param>
        /// <returns></returns>
         public static T Get<T>(object[] initObjects = null)
        {
            var typ = typeof(T);

            if (!Instance._services.ContainsKey(typ))
            {
                throw new Exception(string.Format("There is no registered implementation for type: {0}", typ.FullName));
            }

            //check to see if there is a cached element
            if (initObjects == null && Instance.CachedServices.ContainsKey(typ))
                return (T)Instance.CachedServices[typ];

            var imp = Instance._services[typ];

            var conts = imp.GetTypeInfo().GetConstructors();
            var cachedAttribute = imp.GetTypeInfo().GetCustomAttribute<SingletonServiceAttribute>();

            var cPars = new List<object>();

            if (conts.Length > 0)
            {
                if (initObjects?.Length > 0)
                {
                    foreach (var aConst in conts)
                    {
                        var pars = aConst.GetParameters();

                        if (pars.Length == initObjects.Length)  //TODO:  Should probably check the type of the parameters as well where the number of parameters is the same
                        {
                            //this is the constructor that has the same number of parameters as the initilization objects
                            foreach (var aParam in initObjects)
                                cPars.Add(aParam);
                        }
                    }
                }
                else
                {
                    var ctr = conts[0];

                    var pars = ctr.GetParameters();

                    if (pars.Length != 0)
                    {
                        foreach (var aPar in pars)
                        {
                            var pType = aPar.ParameterType;

                            //see if there is an implementation of the type for the parameter of the consturctor
                            var pImp = Get(pType);

                            if (pImp != null)
                                cPars.Add(pImp); //add as a parameter
                        }
                    }
                }

            }
            var inst = (cPars.Count == 0) ? (T)Activator.CreateInstance(imp) : (T)Activator.CreateInstance(imp, cPars.ToArray());

            if (Instance._constructorActions.ContainsKey(typ))
            {
                var action = Instance._constructorActions[typ];
                action.Invoke(new ConstructorOptions()
                {
                    Context = inst
                });
            }

            //if the impementation is cached, but not yet stored in the cache then add it to the cache
            if ((cachedAttribute != null || Instance._singleTons.Contains(typ)) && !Instance.CachedServices.ContainsKey(typ))
                Instance.CachedServices.Add(typ, inst);

            return inst;

        }

        [Obsolete("Use Get(Type) instead")]
        /// <summary>
        /// Find the implementing service for the speficied interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object Service(Type type) => Get(type);

        /// <summary>
        /// Find the implementing service for the speficied interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object Get(Type type)
        {
            var typ = type;

            if (!Instance._services.ContainsKey(typ))
            {
                return null;
            }

            var imp = Instance._services[typ];

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

                        var pImp = Get(pType);

                        if (pImp != null)
                            cPars.Add(pImp);
                    }
                }

            }

            var inst = (cPars.Count == 0) ? Activator.CreateInstance(imp) : Activator.CreateInstance(imp, cPars.ToArray());

            return inst;

        }

        #endregion

        #region Private

        /// <summary>
        /// Register a service and its corresponding implementation
        /// </summary>
        /// <returns>The register.</returns>
        /// <param name="sInterface">The service interface type</param>
        /// <param name="sImplementation">The service implementation</param>
        /// <param name="constructorAction">The constructor action to call after the instance is created</param>
        private static void Register(Type sInterface, Type sImplementation, Action<object> constructorAction = null)
        {
            if (!sInterface.GetTypeInfo().IsInterface)
                throw new ArgumentException(String.Format("You cannot register {0} as a service interface in ServiceRegistrar as it is not an interface", sInterface.FullName));

            if (Instance._services.ContainsKey(sInterface))
                Instance._services[sInterface] = sImplementation;
            else
                Instance._services.Add(sInterface, sImplementation);

            if (constructorAction != null)
            {
                if (Instance._constructorActions.ContainsKey(sInterface))
                    Instance._constructorActions[sInterface] = constructorAction;
                else
                    Instance._constructorActions.Add(sInterface, constructorAction);
            }


        }


        private static Assembly[] LoadAssemblies(List<AssemblyName> assemblyNames)
        {
            var assemblies = new List<Assembly>();

            foreach (var aItem in assemblyNames)
            {

                var asm = Assembly.Load(aItem);

                assemblies.Add(asm);
            }

            return assemblies.ToArray();
        }

        #endregion
        #endregion
    }
}
