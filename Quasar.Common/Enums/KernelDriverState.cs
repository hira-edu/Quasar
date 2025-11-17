namespace Quasar.Common.Enums
{
    /// <summary>
    /// Represents the runtime state of the capture kernel driver.
    /// </summary>
    public enum KernelDriverState
    {
        /// <summary>
        /// State is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Driver is not installed on the endpoint.
        /// </summary>
        NotInstalled = 1,

        /// <summary>
        /// Driver components are installed but not active.
        /// </summary>
        Installed = 2,

        /// <summary>
        /// Driver is running and hooked.
        /// </summary>
        Running = 3,

        /// <summary>
        /// Driver failed to initialize or crashed.
        /// </summary>
        Failed = 4,

        /// <summary>
        /// Driver has been disabled (for example, operator request).
        /// </summary>
        Disabled = 5
    }
}
