namespace Quasar.Client.Services
{
    /// <summary>
    /// Describes how the client binary is hosting itself.
    /// </summary>
    public enum RuntimeMode
    {
        Interactive,
        Service,
        Watchdog
    }

    /// <summary>
    /// Tracks runtime mode so UI/service components can adjust behavior.
    /// </summary>
    internal static class RuntimeEnvironment
    {
        public static RuntimeMode Mode { get; private set; } = RuntimeMode.Interactive;

        public static bool IsService => Mode == RuntimeMode.Service;

        public static bool IsWatchdog => Mode == RuntimeMode.Watchdog;

        public static void SetMode(RuntimeMode mode)
        {
            Mode = mode;
        }
    }
}
