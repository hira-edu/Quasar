namespace Quasar.Common.Enums
{
    /// <summary>
    /// Describes which action the client should perform against the kernel driver.
    /// </summary>
    public enum KernelDriverAction
    {
        /// <summary>
        /// No action specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Query the current driver state without changing it.
        /// </summary>
        QueryStatus = 1,

        /// <summary>
        /// Ensure the driver is installed and running before continuing.
        /// </summary>
        EnsureRunning = 2,

        /// <summary>
        /// Attempt to reload or restart the driver service.
        /// </summary>
        Restart = 3,

        /// <summary>
        /// Remove any installed driver artifacts.
        /// </summary>
        Remove = 4
    }
}
