namespace Quasar.Server.Helper
{
    /// <summary>
    /// Provides the default process names exposed by the kernel unblock UI.
    /// </summary>
    internal static class KernelUnblockPresets
    {
        /// <summary>
        /// Known Windows components that commonly opt out of capture.
        /// </summary>
        public static readonly string[] ProcessNames = {
            "explorer",
            "RuntimeBroker",
            "ShellExperienceHost",
            "dwm"
        };
    }
}
