using Quasar.Client.Services;
using System;
using System.Linq;
using System.ServiceProcess;

namespace Quasar.Client
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var mode = DetermineMode(args, out var watchdogTarget);
            RuntimeEnvironment.SetMode(mode);

            switch (mode)
            {
                case RuntimeMode.Service:
                    ServiceBase.Run(new QuasarService(args));
                    break;
                case RuntimeMode.Watchdog:
                    ServiceBase.Run(new WatchdogService(watchdogTarget));
                    break;
                default:
                    ClientRuntime.Run(args);
                    break;
            }
        }

        private static RuntimeMode DetermineMode(string[] args, out string watchdogTarget)
        {
            watchdogTarget = null;
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    if (IsFlag(arg, "service"))
                        return RuntimeMode.Service;

                    if (IsFlag(arg, "watchdog"))
                    {
                        if (TryReadNextArg(args, i, out var target))
                        {
                            watchdogTarget = target;
                            i++;
                        }
                        return RuntimeMode.Watchdog;
                    }

                    if (IsFlag(arg, "interactive"))
                        return RuntimeMode.Interactive;
                }
            }

            // Default to service mode unless explicitly overridden. This ensures unattended deployments
            // always register/run as a Windows service.
            return RuntimeMode.Service;
        }

        private static bool IsFlag(string value, string flag) =>
            value.StartsWith("--", StringComparison.OrdinalIgnoreCase) && string.Equals(value.Substring(2), flag, StringComparison.OrdinalIgnoreCase);

        private static bool TryReadNextArg(string[] args, int currentIndex, out string value)
        {
            value = null;
            var nextIndex = currentIndex + 1;
            if (args == null || nextIndex >= args.Length)
                return false;
            value = args[nextIndex];
            return true;
        }
    }
}
