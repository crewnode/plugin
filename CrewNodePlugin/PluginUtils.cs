using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CrewNodePlugin
{
    public static class PluginUtils
    {
        public static void RunTask(Action action, int seconds, CancellationToken token)
        {
            if (action == null) return;
            Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    action();
                    await Task.Delay(TimeSpan.FromSeconds(seconds), token);
                }
            }, token);
        }
    }

    public static class ExtensionMethods
    {
        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            dynamic awaitable = @this.Invoke(obj, parameters);
            await awaitable;
            return awaitable.GetAwaiter().GetResult();
        }
    }
}
