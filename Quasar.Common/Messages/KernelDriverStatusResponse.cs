using ProtoBuf;
using Quasar.Common.Enums;

namespace Quasar.Common.Messages
{
    /// <summary>
    /// Sent by the client to describe the current state of the kernel driver/watchdog pipeline.
    /// </summary>
    [ProtoContract]
    public class KernelDriverStatusResponse : IMessage
    {
        [ProtoMember(1)]
        public KernelDriverState State { get; set; } = KernelDriverState.Unknown;

        [ProtoMember(2)]
        public string Version { get; set; }

        [ProtoMember(3)]
        public bool WatchdogActive { get; set; }

        [ProtoMember(4)]
        public string Message { get; set; }
    }
}
