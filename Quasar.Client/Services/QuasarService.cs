using Quasar.Client.Config;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace Quasar.Client.Services
{
    internal sealed class QuasarService : ServiceBase
    {
        private Thread _workerThread;
        private readonly string[] _args;

        public QuasarService(string[] args)
        {
            _args = args ?? Array.Empty<string>();
            ServiceName = !string.IsNullOrWhiteSpace(Settings.SERVICENAME)
                ? Settings.SERVICENAME
                : "QuasarClientService";
            CanStop = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            _workerThread = new Thread(() =>
            {
                try
                {
                    ClientRuntime.Run(_args);
                }
                catch (Exception)
                {
                    try
                    {
                        EventLog.WriteEntry(ServiceName, "Quasar client service terminated unexpectedly.", EventLogEntryType.Error);
                    }
                    catch
                    {
                    }
                    throw;
                }
            })
            {
                IsBackground = true,
                Name = "QuasarClientServiceThread"
            };
            _workerThread.SetApartmentState(ApartmentState.STA);
            _workerThread.Start();
        }

        protected override void OnStop()
        {
            try
            {
                Application.ExitThread();
            }
            catch
            {
            }

            if (_workerThread != null && _workerThread.IsAlive)
            {
                _workerThread.Join(TimeSpan.FromSeconds(5));
            }
        }
    }
}
