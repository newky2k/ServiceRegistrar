using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DSoft.ServiceRegistrar
{
    /// <summary>
    /// Dependency Injection and Service Management Container
    /// </summary>
    public class Services
    {

        #region Fields
        /// <summary>
        /// Stored instances of the 
        /// </summary>
        private static Dictionary<Type, object> _cachedServices { get; set; } = new Dictionary<Type, object>();

        private static Dictionary<Type, Type> _explicitlyTypedServices { get; set; } = new Dictionary<Type, Type>();

        private static Dictionary<Type, Action<object>> _constructorActions = new Dictionary<Type, Action<object>>();
        /// <summary>
        /// Register types
        /// </summary>
        private static List<Type> _serviceTypes { get; set; } = new List<Type>();

        #endregion

        #region Public members

        /// <summary>
        /// Registers all services marked with the DiscoverableService attribute in the specified assemblies
        /// </summary>
        public static void Register()
        {
            var assm = Assembly.GetCallingAssembly();

            LoadServices(new Assembly[] { assm });
        }

        /// <summary>
        /// Register a service instance
        /// </summary>
        /// <typeparam name="T">Service implementation type</typeparam>
        public static void Register<T>(Action<T> postConstructionAction = null) where T : class, new()
        {
            var newType = typeof(T);

            Action<object> executor = null;

            if (postConstructionAction != null)
                executor = new Action<object>((obj) => { postConstructionAction((T)obj); });

            AddService(newType, postConstructionAction: executor);

        }


        /// <summary>
        /// Registers the specified post construction action.
        /// </summary>
        /// <typeparam name="T">The interface type.</typeparam>
        /// <typeparam name="T2">The implementation type.</typeparam>
        /// <param name="postConstructionAction">The post construction action.</param>
        /// <exception cref="Exception">The first type must be an interface when calling Register&lt;T,T2&gt;.</exception>
        public static void Register<T, T2>(Action<T2> postConstructionAction = null) where T2 : class, new()
        {
            var interfaceType = typeof(T);

            if (!interfaceType.IsInterface)
                throw new Exception("The first type must be an interface when calling Register<T,T2>");

            var implementationType = typeof(T2);

            Action<object> executor = null;

            if (postConstructionAction != null)
                executor = new Action<object>((obj) => { postConstructionAction((T2)obj); });

            AddService(implementationType, interfaceType, executor);
        }

        /// <summary>
        /// Register all services marked with the DiscoverableService attribute in the specified assemblies
        /// </summary>
        /// <param name="assemblies">Array of external assemblies</param>
        public static void Register(params Assembly[] assemblies)
        {
            var assms = new List<Assembly>() { Assembly.GetCallingAssembly() };

            if (assemblies != null && assemblies.Length > 0)
            {
                foreach (var aAssm in assemblies)
                {
                    if (!assms.Contains(aAssm))
                        assms.Add(aAssm);
                }
            }

            LoadServices(assms);
        }

        /// <summary>
        ///  Register all Services in the assemblies conatining the specified types
        /// </summary>
        /// <param name="types">Types to process in external assemblies</param>
        public static void Register(params Type[] types)
        {
            foreach (var type in types)
                AddService(type);
        }

        /// <summary>
        /// Get a service implementation
        /// </summary>
        /// <typeparam name="T">The inherited type</typeparam>
        /// <param name="initObjects">The initialization objects, instead of dependency injection</param>
        /// <returns></returns>
        public static T Get<T>(object[] initObjects = null)
        {
            var type = FindImplementation<T>();

            //if the implentation type is null then it must not have been rergistered so through an error
            if (type == null)
                throw new Exception(string.Format("There is no registered implementation for type: {0}", typeof(T).Name));

            var cachedAttribute = type.GetTypeInfo().GetCustomAttribute<SingletonServiceAttribute>();

            if (cachedAttribute == null)
                return CreateInstance<T>(type, initObjects);

            if (_cachedServices.ContainsKey(type))
            {
                return (T)_cachedServices[type];
            }
            else
            {
                var newType = CreateInstance<T>(type, initObjects);

                _cachedServices.Add(type, newType);

                return newType;
            }

        }

        #endregion

        #region Private members
        private static Type FindImplementation<T>()
        {
            Type type = null;

            //get the request type
            var reqType = typeof(T);

            //check to see if there is an explicitly type service entry
            if (_explicitlyTypedServices.ContainsKey(reqType))
                type = _explicitlyTypedServices[reqType];

            //if there is not explicityly type service see if there an matched type or assginable type in service types
            if (type == null)
                type = _serviceTypes.FirstOrDefault(x => x.Equals(reqType) || reqType.IsAssignableFrom(x));

            return type;
        }

        private static Type FindImplementation(Type reqType)
        {
            Type type = null;

            //check to see if there is an explicitly type service entry
            if (_explicitlyTypedServices.ContainsKey(reqType))
                type = _explicitlyTypedServices[reqType];

            //if there is not explicityly type service see if there an matched type or assginable type in service types
            if (type == null)
                type = _serviceTypes.FirstOrDefault(x => x.Equals(reqType) || reqType.IsAssignableFrom(x));

            return type;
        }

        private static void LoadServices(IEnumerable<Assembly> assemblies)
        {
            var custAttr = typeof(DiscoverableServiceAttribute);

            foreach (var assembly in assemblies)
            {
                var serAttrs = assembly.GetCustomAttributes(custAttr, true);

                foreach (DiscoverableServiceAttribute attrib in serAttrs)
                {
                    AddService(attrib.Implementation, attrib.Interface);

                }


            }
        }

        private static void AddService(Type implementationType, Type interfaceType = null, Action<object> postConstructionAction = null)
        {
            if (interfaceType == null)
            {
                if (!_serviceTypes.Contains(implementationType))
                    _serviceTypes.Add(implementationType);

                if (postConstructionAction != null)
                {
                    if (_constructorActions.ContainsKey(implementationType))
                        _constructorActions[implementationType] = postConstructionAction;
                    else
                        _constructorActions.Add(implementationType, postConstructionAction);
                }
            }
            else
            {
                if (!_explicitlyTypedServices.ContainsKey(interfaceType))
                    _explicitlyTypedServices.Add(interfaceType, implementationType);

                if (postConstructionAction != null)
                {
                    if (_constructorActions.ContainsKey(interfaceType))
                        _constructorActions[interfaceType] = postConstructionAction;
                    else
                        _constructorActions.Add(interfaceType, postConstructionAction);
                }
            }



        }

        #endregion

        #region Private

        private static T CreateInstance<T>(Type type, object[] initObjects = null)
        {
            var conts = type.GetTypeInfo().GetConstructors();

            var cPars = new List<object>();

            if (conts.Length > 0)
            {

                if (initObjects?.Length > 0)
                {
                    foreach (var aConst in conts)
                    {
                        var pars = aConst.GetParameters();

                        if (pars.Length == initObjects.Length) 
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

            var instance = (cPars.Count == 0) ? (T)Activator.CreateInstance(type) : (T)Activator.CreateInstance(type, cPars.ToArray());

            if (_constructorActions.ContainsKey(typeof(T)))
            {
                var action = _constructorActions[typeof(T)];
                action.Invoke(instance);
            }

            return instance;
        }


        private static object CreateInstance(Type type)
        {
            var conts = type.GetTypeInfo().GetConstructors();

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

                        //see if there is an implementation of the type for the parameter of the consturctor
                        var pImp = Get(pType);

                        if (pImp != null)
                            cPars.Add(pImp); //add as a parameter
                    }
                }

            }

            var instance = (cPars.Count == 0) ? Activator.CreateInstance(type) : Activator.CreateInstance(type, cPars.ToArray());

            return instance;
        }


        private static object Get(Type intefaceType)
        {
            var type = FindImplementation(intefaceType);

            //if the implentation type is null then it must not have been rergistered so through an error
            if (type == null)
                throw new Exception(string.Format("There is no registered implementation for type: {0}", intefaceType.Name));

            var cachedAttribute = type.GetTypeInfo().GetCustomAttribute<SingletonServiceAttribute>();

            if (cachedAttribute == null)
                return CreateInstance(type);

            if (_cachedServices.ContainsKey(type))
            {
                return _cachedServices[type];
            }
            else
            {
                var newType = CreateInstance(type);

                _cachedServices.Add(type, newType);

                return newType;
            }

        }

        #endregion

    }
}
