using Quasar.Client.Helper;
using Quasar.Client.Logging;
using Quasar.Common.Enums;
using Quasar.Common.Messages;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Quasar.Client.RemoteDesktop
{
    /// <summary>
    /// Provides a best-effort workflow for restoring remote mouse/keyboard input.
    /// </summary>
    internal sealed class InputUnblockCommand
    {
        private readonly KernelDriverLogger _logger;
        private const int DefaultAttempts = 3;
        private const int RetryDelayMs = 200;

        public InputUnblockCommand(KernelDriverLogger logger)
        {
            _logger = logger ?? new KernelDriverLogger();
        }

        public InputUnblockResult Execute(DoInputUnblock request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var stopwatch = Stopwatch.StartNew();
            var details = new StringBuilder();

            bool blockResetSuccess = true;
            int blockResetError = 0;

            if (request.UnblockMouse || request.UnblockKeyboard)
            {
                blockResetSuccess = NativeMethodsHelper.TryResetBlockInput(DefaultAttempts, RetryDelayMs, out blockResetError);
                if (!blockResetSuccess)
                {
                    details.AppendLine($"BlockInput reset failed (0x{blockResetError:X}).");
                    _logger.Warning($"Input unblock: BlockInput(FALSE) failed after {DefaultAttempts} attempt(s), error 0x{blockResetError:X}.");
                }
                else
                {
                    details.AppendLine("BlockInput reset succeeded.");
                }
            }

            stopwatch.Stop();

            return new InputUnblockResult
            {
                ResultCode = blockResetSuccess ? InputUnblockResultCode.Success : InputUnblockResultCode.BlockInputFailed,
                MouseUnlocked = request.UnblockMouse,
                KeyboardUnlocked = request.UnblockKeyboard,
                Message = details.ToString().Trim(),
                DurationMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
