using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using Newtonsoft.Json;

namespace GodotManager.Library.Utility;

internal class AppModule
{
    [ModuleInitializer]
    public static void Initialize()
    {
        AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly()).Unloading += alc =>
        {
            var assembly = typeof(JsonSerializerOptions).Assembly;
            var updateHandlerType = assembly.GetType("System.Text.Json.JsonSerializerOptionsUpdateHandler");
            var clearCacheMethod =
                updateHandlerType?.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public);
            clearCacheMethod?.Invoke(null, new object?[] { null });
        };

    }
}