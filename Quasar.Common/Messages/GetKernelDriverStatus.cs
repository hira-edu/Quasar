using ProtoBuf;
using Quasar.Common.Enums;

namespace Quasar.Common.Messages
{
    /// <summary>
    /// Requests the current kernel driver state from a client.
    /// </summary>
    [ProtoContract]
    public class GetKernelDriverStatus : IMessage
    {
        [ProtoMember(1)]
        public KernelDriverAction DriverAction { get; set; } = KernelDriverAction.QueryStatus;

        [ProtoMember(2)]
        public bool ForceRefresh { get; set; }
    }
}
