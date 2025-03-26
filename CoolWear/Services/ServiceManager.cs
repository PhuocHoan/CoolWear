using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolWear.Services;

public sealed class ServiceManager
{
    private static readonly Dictionary<string, object> _singletons = [];
    public static void AddKeyedSingleton<IParent, Child>()
    {
        Type parent = typeof(IParent);
        Type child = typeof(Child);
        _singletons[parent.Name] = Activator.CreateInstance(child)!;
    }

    public static void AddKeyedSingleton<IService>(Func<IService> factory)
    {
        Type serviceType = typeof(IService);
        _singletons[serviceType.Name] = factory()!;
    }



    public static IParent GetKeyedSingleton<IParent>()
    {
        Type parent = typeof(IParent);
        return (IParent)_singletons[parent.Name];
    }
}
