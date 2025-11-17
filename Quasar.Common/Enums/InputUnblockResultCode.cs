namespace Quasar.Common.Enums
{
    /// <summary>
    /// Indicates the outcome of an input-unblock operation.
    /// </summary>
    public enum InputUnblockResultCode
    {
        /// <summary>
        /// Result is unknown or not reported.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Input was successfully unblocked.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Input was already unblocked prior to the request.
        /// </summary>
        AlreadyUnlocked = 2,

        /// <summary>
        /// Resetting BlockInput failed.
        /// </summary>
        BlockInputFailed = 3,

        /// <summary>
        /// Removing low-level hooks or reattaching threads failed.
        /// </summary>
        HookRemovalFailed = 4,

        /// <summary>
        /// Operation failed due to access-denied/privilege issues.
        /// </summary>
        AccessDenied = 5,

        /// <summary>
        /// The endpoint is on a secure desktop (Winlogon/UAC) where unblocking is not allowed.
        /// </summary>
        SecureDesktop = 6,

        /// <summary>
        /// An unexpected error prevented unblocking.
        /// </summary>
        Failed = 7
    }
}
