#nullable enable
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;

namespace GodotManager.Library.Util;

internal class AppModule
{
    [ModuleInitializer]
    public static void Initialize()
    {
        AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())!.Unloading += alc =>
        {
            var assembly = typeof(JsonSerializerOptions).Assembly;
            var updateHandlerType = assembly.GetType("System.Text.Json.JsonSerializerUpdateHandler");
            var clearHandlerMethod =
                updateHandlerType?.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public);
            clearHandlerMethod?.Invoke(null, new object?[] { null });
        };
    }

}
