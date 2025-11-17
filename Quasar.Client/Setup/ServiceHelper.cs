using Quasar.Client.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace Quasar.Client.Setup
{
    internal static class ServiceHelper
    {
        public static void InstallOrUpdateService(string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                return;

            var serviceName = !string.IsNullOrWhiteSpace(Settings.SERVICENAME)
                ? Settings.SERVICENAME
                : Path.GetFileNameWithoutExtension(executablePath);

            var displayName = !string.IsNullOrWhiteSpace(Settings.SERVICEDISPLAYNAME)
                ? Settings.SERVICEDISPLAYNAME
                : serviceName;

            InstallOrRefreshService(serviceName, displayName, BuildBinPath(executablePath, "--service"));

            var watchdogName = $"{serviceName}Watchdog";
            var watchdogDisplay = $"{displayName} Watchdog";
            InstallOrRefreshService(watchdogName, watchdogDisplay, BuildBinPath(executablePath, $"--watchdog \"{serviceName}\""));
        }

        private static string BuildBinPath(string executablePath, string arguments)
        {
            var quotedExe = $"\"{executablePath}\"";
            var command = string.IsNullOrWhiteSpace(arguments) ? quotedExe : $"{quotedExe} {arguments}";
            return $"\"{command}\"";
        }

        private static void InstallOrRefreshService(string serviceName, string displayName, string binPath)
        {
            bool exists = ServiceExists(serviceName);

            if (exists)
            {
                ExecuteSc($@"stop ""{serviceName}""");
                WaitForServiceState(serviceName, ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                ExecuteSc($@"config ""{serviceName}"" binPath= {binPath} start= auto DisplayName= ""{displayName}""");
            }
            else
            {
                ExecuteSc($@"create ""{serviceName}"" binPath= {binPath} start= auto DisplayName= ""{displayName}""");
            }

            ExecuteSc($@"description ""{serviceName}"" ""{displayName}""");
            ExecuteSc($@"failure ""{serviceName}"" reset= 0 actions= restart/5000/restart/5000/restart/5000");
            ExecuteSc($@"failureflag ""{serviceName}"" 1");
            ExecuteSc($@"start ""{serviceName}""");
        }

        private static bool ServiceExists(string serviceName)
        {
            try
            {
                return ServiceController.GetServices().Any(s =>
                    string.Equals(s.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        private static void WaitForServiceState(string serviceName, ServiceControllerStatus desiredState, TimeSpan timeout)
        {
            try
            {
                using (var controller = new ServiceController(serviceName))
                {
                    controller.WaitForStatus(desiredState, timeout);
                }
            }
            catch
            {
            }
        }

        private static void ExecuteSc(string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process?.WaitForExit(10000);
                }
            }
            catch
            {
            }
        }
    }
}
