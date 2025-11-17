namespace Quasar.Common.Enums
{
    /// <summary>
    /// Indicates the outcome of a kernel unblock attempt.
    /// </summary>
    public enum KernelUnblockResultCode
    {
        /// <summary>
        /// Result unspecified.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Windows were reset successfully.
        /// </summary>
        Success = 1,

        /// <summary>
        /// No running process matched the supplied name.
        /// </summary>
        NoMatchingProcess = 2,

        /// <summary>
        /// The driver was not available or could not be prepared.
        /// </summary>
        DriverUnavailable = 3,

        /// <summary>
        /// The command failed due to missing permissions.
        /// </summary>
        AccessDenied = 4,

        /// <summary>
        /// An unexpected error prevented unblocking.
        /// </summary>
        Failed = 5
    }
}
