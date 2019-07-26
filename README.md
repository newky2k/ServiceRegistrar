# DSoft.ServiceRegistrar.Core

Dependency injection and service registration container

Features:

- Simple service registration
  - Register Singletons
- Dependency injection of registered services during instantiation
  - Passing of initialization parameters
  - Post-Instantiation actions 
- Auto Discovery using reflection
  - Auto-Register singleton implementations using `SingletonServiceAttribute`
- .NET Standard 2.0 

## Usage

### AutoDiscovery
`ServiceRegistrar.RegisterWithAutoDiscovery` will process the specified assembly to auto-register any implementations of any interfaces that inherit from `IAutoDiscoverableProvider`

    using DSoft.ServiceRegistrar;
    ...
    ServiceRegistrar.RegisterWithAutoDiscovery(typeof(TestImplementation2).Assembly);

**Note: if the implementation class is decorated with the `SingletonServiceAttribute` then it will configured as a singleton and cached after the first instantiation.**

### Register Implementations in an Assembly
There are several methods for registering the implementations contained within assemblies.

These are:

- `ServiceRegistrar.RegisterFromAssemblies(Assembly interfaces, Assembly implementations)`
  - Registers the implementations in the implementations assembly that implements the interfaces in the interfaces assembly
- `ServiceRegistrar.RegisterFromAssemblies<T, T2>()`
  - Registers the implementations in the assembly containing the type of `T2` that implements the interfaces in the assembly that contains the type of `T`
- `ServiceRegistrar.RegisterFromCallingAssembly(Assembly interfaceAssembly)`
    - Processes the calling assembly and matches implementations of any matching interfaces in the interfaces assembly
- `ServiceRegistrar.RegisterFromCallingAssembly<T>()`
    - Processes the calling assembly and matches implementations of any matching interfaces in the interfaces assembly containing `T`

**Note: if the implementation class is decorated with the `SingletonServiceAttribute` then it will configured as a singleton and cached after the first instantiation.**

### Register

You can register an individual interface/implementation combination using `ServiceRegistrar.Register<T,T2>()`

    using DSoft.ServiceRegistrar;
    ...
    ServiceRegistrar.Register<ITestInterface4, TestImplementation4>();

You can also specify a post-construcion action to execute after the implementation has been instantiated.

    using DSoft.ServiceRegistrar;
    ...
    ServiceRegistrar.Register<ITestInterface4, TestImplementation4>(obj =>
            {
                ((TestImplementation4)obj.Context).Message = "Hello, init!";
            });

You can also register a singleton instance using `ServiceRegistrar.RegisterSingleton<T,T2>()`, with or without a post-construcion action.

    using DSoft.ServiceRegistrar;
    ...
    ServiceRegistrar.RegisterSingleton<ITestInterface4, TestImplementation4>(obj =>
            {
                ((TestImplementation4)obj.Context).Message = "Hello, init!";
            });


### Service
To access the implementation of a interface you call `ServiceRegistrar.Service<T>()`.  This will return an instance of the implementation of the interface.  

    using DSoft.ServiceRegistrar;
    ...
    var serv4 = ServiceRegistrar.Service<ITestInterface4>();
    serv4.DoAThing();

You can also pass through initialisation objects that can be passed to a matching constructor within the implementation class.

    using DSoft.ServiceRegistrar;
    ...
    var serv = ServiceRegistrar.Service<ITestInterface2>();

    var result = serv.TestInt();

    var serv2 = ServiceRegistrar.Service<TestInterface3>(new object[] { serv });

*If the service is a singleton and a post-construcion action is defined, it will be executed the first time the implementation is instantiated.*

### Dependency Injection
ServiceRegistrar can inject dependencies on the other interface implementations, but only under the following conditions.

- There can be only one constructor on the implementing class of the service being requested
  - It can have more than one parameter but they must be references to other registered interfaces
- Each implementating class, that is being injected, can have only one constructor that:
  - Either has no parameters 
  - Or references other registered interface implementations
- Will not work with intitilization objects in `ServiceRegistrar.Service<T>()`

**Note: Work will be done on the Depdency Injection to remove some of the limitations**
