using Quasar.Client.Config;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace Quasar.Client.Services
{
    internal sealed class WatchdogService : ServiceBase
    {
        private readonly string _targetServiceName;
        private Timer _monitorTimer;

        public WatchdogService(string targetServiceName)
        {
            _targetServiceName = string.IsNullOrWhiteSpace(targetServiceName)
                ? (!string.IsNullOrWhiteSpace(Settings.SERVICENAME) ? Settings.SERVICENAME : "QuasarClientService")
                : targetServiceName;

            ServiceName = $"{_targetServiceName}Watchdog";
            CanStop = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            _monitorTimer = new Timer(MonitorService, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
        }

        protected override void OnStop()
        {
            _monitorTimer?.Dispose();
        }

        private void MonitorService(object state)
        {
            try
            {
                using (var controller = new ServiceController(_targetServiceName))
                {
                    controller.Refresh();
                    if (controller.Status == ServiceControllerStatus.Stopped ||
                        controller.Status == ServiceControllerStatus.StopPending)
                    {
                        controller.Start();
                        controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    EventLog.WriteEntry(ServiceName, $"Watchdog encountered an error: {ex.Message}", EventLogEntryType.Warning);
                }
                catch
                {
                }
            }
        }
    }
}
