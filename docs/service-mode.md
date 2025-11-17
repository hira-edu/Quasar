## Quasar Client Service Mode

The Quasar client now runs exclusively as a Windows service with an optional watchdog companion. This section explains how the components are installed, what privileges are required, and how the new mode differs from the legacy tray-based client.

### Components

- **Primary service**: installs as `QuasarClientService` (name/display name are customizable through the builder) and hosts the original client runtime headlessly. The executable is launched with the `--service` argument so the entry point bypasses all UI setup.
- **Watchdog service**: installs as `<PrimaryName>Watchdog` and launches the same binary with the `--watchdog <PrimaryName>` arguments. It polls the primary service every 15 seconds and restarts it if it stops unexpectedly.
- Both services configure SC failure actions (three restarts, five seconds apart) and are set to `start= auto` so they come online during boot before any user logon.

### Installation Flow

1. The builder emits service names/display names alongside the rest of the client configuration.
2. When the client runs `ClientInstaller.ApplySettings()`, it:
   - Copies the executable to the configured install path.
   - Applies file hiding/startup settings (if requested).
   - Calls `ServiceHelper.InstallOrUpdateService`, which uses `sc.exe` to create/update:
     - `"<install path>" --service`
     - `"<install path>" --watchdog "<PrimaryServiceName>"`
   - Configures service descriptions, restart policies, and starts both services.
3. Subsequent runs update the service configuration in-place without deleting/recreating the service entries.

### Permissions & Requirements

- Installation requires administrative privileges (the helper invokes `sc.exe create/config/...`).
- The binary must reside on a fixed disk path (not a network share) so the SCM can load it during boot.
- Because the service hosts a WinForms-based application off-screen, the Windows Service Control Manager must be allowed to interact with the desktop (default on supported OS versions).

### Differences from Tray Mode

- No notification icon or on-screen UI is shown even when a user is logged in.
- The client always enforces unattended mode (`Settings.UNATTENDEDMODE = true`) when running as a service.
- Restarts are handled by the watchdog + SCM failure actions rather than per-user tray restarts.
- Operators must stop the services through `sc stop`, Services MMC, or the builder’s uninstall flow instead of exiting a tray application.

### Operations & Troubleshooting

- Use `sc query QuasarClientService` / `QuasarClientServiceWatchdog` to inspect status.
- Event Log entries (Application log) record unexpected crashes and watchdog recovery attempts.
- To uninstall, stop both services and delete them (`sc delete`), then remove the install directory.
- If the watchdog can’t restart the primary service (for example due to configuration errors), check the event log entry emitted by `WatchdogService`.
