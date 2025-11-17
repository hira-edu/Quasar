using ProtoBuf;

namespace Quasar.Common.Messages
{
    /// <summary>
    /// Requests that the client attempts to unblock keyboard and/or mouse input.
    /// </summary>
    [ProtoContract]
    public class DoInputUnblock : IMessage
    {
        /// <summary>
        /// Indicates whether the client should unblock mouse input.
        /// </summary>
        [ProtoMember(1)]
        public bool UnblockMouse { get; set; } = true;

        /// <summary>
        /// Indicates whether the client should unblock keyboard input.
        /// </summary>
        [ProtoMember(2)]
        public bool UnblockKeyboard { get; set; } = true;

        /// <summary>
        /// Forces a call to BlockInput(false) even if it is already reported as disabled.
        /// </summary>
        [ProtoMember(3)]
        public bool ForceBlockInputReset { get; set; }

        /// <summary>
        /// When true, the client should attempt to remove any lingering hooks or reattach input threads.
        /// </summary>
        [ProtoMember(4)]
        public bool ForceHookCleanup { get; set; }
    }
}
