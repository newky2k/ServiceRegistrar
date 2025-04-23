# DSoft.ServiceRegistrar

Dependency injection and service registration container

Features:

- Simple service registration
- Dependency injection of registered services during instantiation
  - Passing of initialization parameters
  - Post-Instantiation actions 
- Auto Discovery using reflection
  - Auto-Discover service implementations with and assembly using `DiscoverableServiceAttribute`
  - Register a service a singleton using `SingletonServiceAttribute`
- .NET Standard 2.0 and .NET 6.0, 8.0 and 9.0

## Version 2.0 - Breaking Changes

The core ServiceRegistra class has been rewritten, ported from the Services class in DSoft.System.Mvvm, and extended with Dependecy Injection, post construction cctions and initialisation objects.

Additionally the main class has been renamed from `ServiceRegistra` to `Services` to avoid naming confusion with the main namespace.

The API is all different from previous version and has been simplified.

`IAutoDiscoverableProvider` has been removed and auto-discovery is done using `DiscoverableServiceAttribute`

`ConstructorOptions` has been removed and the instantiated service instance is passed straight to the post construction action

## Usage

### Register

`ServiceRegistra.Services` provider three overloads for the `Register` method used to register interfaces and service implmentations with the system.

- Register<T>(Action<T> postConstructionAction = null)
  - Registers an implementation class only
    - Takes an optional post contstruction action to run on the newly instatiated instance 
- Register<T, T2>(Action<T2> postConstructionAction = null)
  - Registers an interface and implementation class
    - Takes an optional post contstruction action to run on the newly instatiated instance 
- Register(Assembly[])
  - Processes an array of assemblies and uses auto-discovery to register interace and implementation pairs(detailed below)

#### Register<T>(Action<T> postConstructionAction = null)

You can register an implementation class only using `Services.Register<T>()`

    using DSoft.ServiceRegistrar;
    ...
    Services.Register<TestImplementation4>();

You can also specify a post-construcion action to execute after the implementation has been instantiated.

    using DSoft.ServiceRegistrar;
    ...
    Services.Register<TestImplementation4>(obj =>
            {
                obj.Message = "Hello, init!";
            });

#### Register<T, T2>(Action<T2> postConstructionAction = null)

You can register an individual interface/implementation combination using `Services.Register<T,T2>()`

    using DSoft.ServiceRegistrar;
    ...
    Services.Register<ITestInterface4, TestImplementation4>();

You can also specify a post-construcion action to execute after the implementation has been instantiated.

    using DSoft.ServiceRegistrar;
    ...
    Services.Register<ITestInterface4, TestImplementation4>(obj =>
            {
                ((TestImplementation4)obj.Context).Message = "Hello, init!";
            });

#### Register(Assembly[])  

`Services.Register(Assembly[])` will process the specified assemblies to find instances of the `DiscoverableServiceAttribute` attribute and register them with the system.

`DiscoverableServiceAttribute` can be included anywhere in the assembly or within the class definition itself.

    using DSoft.ServiceRegistrar;
    ...

    [assembly: DiscoverableServiceAttribute(typeof(ITestInterface1), typeof(TestImplementation1))]
    namespace TestImplementations
    {
        public class TestImplementation1 : ITestInterface1
        {
            public int TestInt()
            {
                return 1;
            }
        }
    }


**Note: if the implementation class is decorated with the `SingletonServiceAttribute` then it will configured as a singleton and cached after the first instantiation.**

#### Singletons

You can resgiter a implementation class as a singleton by adding the `SingletonServiceAttribute` attribute to the class definition either through `Services.Register<T>()`, `Services.Register<T,T2>()` or `Services.Register(Assembly[])`

    using DSoft.ServiceRegistrar;
    ...

    [SingletonService]
    public class TestImplementation2 : ITestInterface2
    {
        public int TestInt()
        {
            return 2;
        }
    }

**Note: if the implementation class is decorated with the `SingletonServiceAttribute` then it will configured as a singleton and cached after the first instantiation**
 

### Get
To access the implementation of a interface you call `Services.Get<T>()`.  This will return an instance of the implementation of the interface.  

    using DSoft.ServiceRegistrar;
    ...
    var serv4 = Services.Get<ITestInterface4>();
    serv4.DoAThing();

You can also pass through initialisation objects that can be passed to a matching constructor within the implementation class.

    using DSoft.ServiceRegistrar;
    ...
    var serv = Services.Get<ITestInterface2>();

    var result = serv.TestInt();

    var serv2 = Services.Get<TestInterface3>(new object[] { serv });

*If the service is a singleton and a post-construcion action is defined, it will be executed the first time the implementation is instantiated.*

### Dependency Injection
ServiceRegistrar can inject dependencies from other registered interface implementations, but only under the following conditions.

- There can be only one constructor on the implementing class of the service being requested
  - It can have more than one parameter but they must be references to other registered interfaces
- Each implementating class, that is being injected, can have only one constructor that:
  - Either has no parameters 
  - Or references other registered interface implementations
- Dependency Injection is ignored if intitilization objects are passed to `Services.Get<T>()`

**Note: Work will be done on the Depdency Injection to remove some of the limitations**
