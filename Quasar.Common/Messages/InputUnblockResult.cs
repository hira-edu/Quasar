using ProtoBuf;
using Quasar.Common.Enums;

namespace Quasar.Common.Messages
{
    /// <summary>
    /// Reports the outcome of an input-unblock request.
    /// </summary>
    [ProtoContract]
    public class InputUnblockResult : IMessage
    {
        [ProtoMember(1)]
        public InputUnblockResultCode ResultCode { get; set; } = InputUnblockResultCode.Unknown;

        [ProtoMember(2)]
        public bool MouseUnlocked { get; set; }

        [ProtoMember(3)]
        public bool KeyboardUnlocked { get; set; }

        [ProtoMember(4)]
        public string Message { get; set; }

        [ProtoMember(5)]
        public long DurationMilliseconds { get; set; }
    }
}
