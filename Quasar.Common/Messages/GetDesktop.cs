using ProtoBuf;

namespace Quasar.Common.Messages
{
    [ProtoContract]
    public class GetDesktop : IMessage
    {
        [ProtoMember(1)]
        public bool CreateNew { get; set; }

        [ProtoMember(2)]
        public int Quality { get; set; }

        [ProtoMember(3)]
        public int DisplayIndex { get; set; }

        /// <summary>
        /// When true, the client should draw the local cursor into the captured frame.
        /// </summary>
        [ProtoMember(4)]
        public bool IncludeCursor { get; set; } = true;

        /// <summary>
        /// Forces the capture routine to reset SetWindowDisplayAffinity before grabbing pixels.
        /// </summary>
        [ProtoMember(5)]
        public bool ForceAffinityReset { get; set; }
    }
}
