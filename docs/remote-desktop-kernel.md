# Remote Desktop Kernel Workflow

This guide outlines how the new kernel unblock flow integrates with the existing Remote Desktop tooling.

## Server UI Additions

- The Remote Desktop toolbar now exposes a **Driver** status label, target selector, and `Unblock` button plus capture-option toggles.
- The status label reports the last `KernelDriverStatusResponse` from the client and surfaces any errors via its tooltip.
- `Show cursor` controls whether the client paints the local pointer into each captured frame.
- `Force unblock pass` instructs the client to call `SetWindowDisplayAffinity(hwnd, 0)` before each capture, even if the window is already captureable.
- `Require driver` toggles whether `DoKernelUnblock` should first ensure the kernel driver is installed/running. Disable this only when you intentionally want a user-mode-only unblock.
- Use the target dropdown to pick from common capture-blocking processes (Explorer, RuntimeBroker, ShellExperienceHost, DWM). Selections are mirrored by the main window context menu.
- Press `Unblock` to send a `DoKernelUnblock` message. The client will optionally ensure the driver is running, clear window affinities, and respond with a `KernelUnblockResult`. The server surfaces the outcome in a modal dialog and refreshes the driver status automatically.
- The `Refresh` button can be used at any time to force a `GetKernelDriverStatus` query.
- Cursor and affinity options sit alongside quality controls. “Show cursor” toggles whether frames include the remote cursor, and “Force unblock pass” requests a best-effort affinity sweep before each capture. Both are remembered per client so operators don’t have to reconfigure on reconnect.
- If the server encounters blank frames, it now escalates aggressively: every blank response forces `Force unblock pass`, replays the full preset list of kernel-unblock requests, and re-queries driver status before issuing the next `GetDesktop`. It keeps “knocking on the door” until a non-blank frame arrives, dramatically boosting capture success without manual intervention.
- A new **Input Unblock** capsule sits below the kernel controls. Choose whether to target mouse and/or keyboard input via the checkboxes, then press `Unblock Input`. The client will attempt to reset `BlockInput` (with up to three automatic retries per request), detach errant hooks, and reply with an `InputUnblockResult`. Status updates appear on the toolbar and detailed results show in a dialog.

## Main Window Context Menu

- `Monitoring → Kernel Unblock` now expands to the same set of target processes. Choosing an entry broadcasts a `DoKernelUnblock` request to every selected client.
- A `Custom...` entry remains available for ad-hoc process names (uses the classic prompt) so operators are not limited to the presets.
- `Monitoring → Input Unblock` sends a broadcast `DoInputUnblock` to each selected client. The default request targets both mouse and keyboard input and forces a BlockInput reset + hook cleanup.

## Operator Notes

- Both entry points rely on the strongly-typed `DoKernelUnblock`, `KernelUnblockResult`, and `KernelDriverStatusResponse` messages defined in `Quasar.Common`. Ensure updated clients are deployed before using the UI.
- Remote input unblocking uses the new `DoInputUnblock`/`InputUnblockResult` messages. Results are surfaced in the Remote Desktop form and can also be seen in the client log file (`kernel-driver.log`) alongside kernel-driver telemetry.
- The UI does not currently stream incremental progress. If a target owns many windows, expect a short pause until the result dialog is displayed.
- The presets live in `Quasar.Server.Helper.KernelUnblockPresets` if you need to add or remove default entries for a deployment.
- `kernel-driver.log` (under `%APPDATA%\Quasar\Logs` by default) stores a rolling trace of driver-service operations and unblock attempts so SOC operators can audit actions post-event.

## Client internals

- `KernelDriverManager` talks to the Windows SCM (`advapi32!OpenSCManager/OpenService/QueryServiceStatusEx`) to make sure the `QuasarRemoteDesktopDrv` service is running before unblocking protected windows. The manager can start/stop/restart the service and gracefully reports when the driver is not installed or when operations time out.
- `KernelUnblockCommand` enumerates every window owned by the target process (via `NativeMethodsHelper.EnumerateProcessWindows`) and uses `user32!SetWindowDisplayAffinity` to force each handle back to `WDA_NONE`. Each run produces a `KernelUnblockResult` summarizing attempts, skips, failures, and duration.
- Both components share `KernelDriverLogger`, ensuring every driver or unblock operation is captured with UTC timestamps in `kernel-driver.log`.
