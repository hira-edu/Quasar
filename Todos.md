## KernelUnblock Command Flow

### Background (reference implementation)
- `xClient.Core.Commands/CommandHandler.cs:567-608` handles `CustomCommandType.KernelUnblock`. A `DoCustomCommand` packet carries the target process name (arg[0]), each PID returned by `Process.GetProcessesByName` is enumerated, and every window tied to those processes is collected through the `_4998CE99` dispatcher exposed as `NativeHelper.CheckDiskUsage`.
- Each window handle is passed to `_4998CE99` → `NativeHelper.Loop`, which ultimately invokes `user32!SetWindowDisplayAffinity(hwnd, 0)` — resetting the affinity back to `WDA_NONE` so the surface becomes captureable again.
- `HandleGetDesktop` (CommandHandler.cs:631-697) switches desktops by calling `NativeMethods.OpenDesktop`/`SetThreadDesktop`, keeps a singleton `UnsafeStreamCodec`, uses `ScreenHelper.CaptureScreen` (BitBlt with `SRCCOPY | CAPTUREBLT`) and overlays the cursor via `GetCursorInfo` + `DrawIcon` before serializing a `GetDesktopResponse`.
- `Downloads/infinity-hook-driver-2adeab8b71b5a65705333cb5bfa457d1c8a4cce7/` (copied from RuntimeBroker2_src) contains `infinity_hook_pro_max.sys` + INF/CAT/PDB. Strings show it hooks `NtUserSetWindowDisplayAffinity`, preventing any process from reinstating `WDA_EXCLUDEFROMCAPTURE`.
- Full loop: C2 issues KernelUnblock → client enumerates windows → user-mode stubs clear affinity → capture routine streams pixels → kernel driver blocks any future affinity changes.

### Phase 1 – Protocol + UI entry points
- [x] `Quasar.Common/Messages`: add strongly typed packets (`DoKernelUnblock`, `GetKernelDriverStatus`, `KernelDriverStatusResponse`, `KernelUnblockResult`) with ProtoBuf contracts and document them in comments so `TypeRegistry` (Client.cs:255, Server.cs:219) automatically discovers them.
- [x] `Quasar.Common/Enums`: introduce a `KernelDriverAction`/`KernelUnblockResultCode` enum to capture driver install/start/unblock outcomes and re-use it on both ends.
- [x] `Quasar.Server/Messages/RemoteDesktopHandler.cs`: dispatch new outbound `DoKernelUnblock` requests, listen for `KernelDriverStatusResponse`, and surface status updates to the UI thread through the existing `SynchronizationContext`.
- [x] `Quasar.Server/Forms/FrmRemoteDesktop.cs` & `.Designer.cs`: add a command bar section (button + dropdown) to trigger KernelUnblock, show driver status (e.g., `Label lblKernelDriverState`), and wire up click handlers that call `_remoteDesktopHandler.SendKernelUnblock(...)`.
- [x] `Quasar.Server/Forms/FrmMain.cs`: expose the same action via the client context menu so operators can request a KernelUnblock even before the remote desktop window is open.
- [x] `docs/remote-desktop-kernel.md`: document the new UI workflow, explaining when to choose user-mode unblocking versus the kernel driver flow.

### Phase 2 – Client command + window enumeration
- [x] `Quasar.Client/Messages/RemoteDesktopHandler.cs`: extend `CanExecute`/`Execute` to understand the new `DoKernelUnblock` and `GetKernelDriverStatus` messages, marshal work off the UI thread, and send back `KernelUnblockResult` packets.
- [x] `Quasar.Client/RemoteDesktop/KernelUnblockCommand.cs` (new): encapsulate the logic for locating target processes, enumerating window handles, calling `SetWindowDisplayAffinity(hwnd, 0)`, and bubbling per-window errors for telemetry.
- [x] `Quasar.Client/RemoteDesktop/Driver/KernelDriverManager.cs` (new): own extraction paths, version checks, and service/start/stop semantics so RemoteDesktopHandler can simply request “ensure driver is running before unblocking.”
- [x] `Quasar.Client/Utilities/NativeMethods.cs`: add the missing P/Invoke coverage required by the above (e.g., `EnumWindows`, `GetWindowThreadProcessId`, `EnumChildWindows`, `SetWindowDisplayAffinity`, `DwmGetWindowAttribute`, `GetCursorInfo`, `DrawIconEx`, `CreateCompatibleBitmap`, `DeleteObject`, SCM APIs if we avoid `sc.exe`).
- [x] `Quasar.Client/Helper/NativeMethodsHelper.cs`: provide safe wrappers (window enumeration, cursor capture, SCM fallback) and unit-testable helpers instead of sprinkling raw handles in message handlers.
- [x] `Quasar.Client/Helper/ScreenHelper.cs`: expose a `CaptureScreenWithOptions(CaptureOptions options)` overload so KernelUnblock can re-use the same capture primitives and we can plug cursor + affinity flags into one place.
- [x] Logging: hook into `Quasar.Client/Logging` to write meaningful driver/SetWindowDisplayAffinity events (use `KeyloggerService` pattern for rotating logs).

### Phase 3 – Capture flow hardening
- [x] `Quasar.Common/Messages/GetDesktop.cs` / `GetDesktopResponse.cs`: include optional fields such as `bool IncludeCursor`, `bool ForceAffinityReset`, `KernelDriverState DriverState`, and a `long FrameId` so the server can display state per frame.
- [x] `Quasar.Client/Messages/RemoteDesktopHandler.cs`: pass the new options through to `ScreenHelper`, keep `_streamCodec` metadata in sync, and emit driver state with every `GetDesktopResponse`.
- [x] `Quasar.Server/Messages/RemoteDesktopHandler.cs`: read the expanded response payload, update `LocalResolution`, and push driver state updates back to `FrmRemoteDesktop` without blocking frame processing.
- [ ] `Quasar.Server/Forms/FrmRemoteDesktop.cs`: surface capture options (cursor toggle, affinity reset, driver requirement) alongside quality/monitor selectors and persist the last choice per client.
- [ ] `Quasar.CaptureService/*`: flesh out the empty scaffolding (Capture/, Driver/, Processing/) into a background service that can own DXGI/Desktop Duplication capture later; ensure it can be invoked from `Quasar.Client` through named pipes for low-latency grabs once the UI migrates off WinForms.
- [ ] Telemetry: add lightweight counters (maybe Prometheus-style JSON dumped via `GetStatus`) so we can observe capture FPS, driver attach latency, and affinity failures.

### Phase 4 – Kernel driver packaging + verification
- [x] `Quasar.Driver.RemoteDesktop`: populate this project with the raw artifacts from `~/Downloads/infinity-hook-driver-2adeab8b71b5a65705333cb5bfa457d1c8a4cce7/` (INF/CAT/SYS/PDB + `build_info.json`) and add a README stating the source commit `2adeab8b71b5a65705333cb5bfa457d1c8a4cce7`.
- [x] `Quasar.Driver.RemoteDesktop/DriverPackage.targets` (new): zip/sign the artifacts during build and expose `@(RemoteDesktopDriverPackage)` items so `Quasar.Client` can consume them as embedded resources.
- [x] `Quasar.Client/Quasar.Client.csproj`: reference `Quasar.Driver.RemoteDesktop`, mark the produced `.zip` as an EmbeddedResource, and ensure ILRepack includes it.
- [ ] `Quasar.Client/Properties/Resources.resx`: add strongly typed entries (`RemoteDesktopDriverZip`, `RemoteDesktopDriverVersion`) that surface the packaged bits to runtime code.
- [ ] `Quasar.Client/RemoteDesktop/Driver/DriverExtractor.cs`: at runtime, drop the embedded payload to `%PROGRAMDATA%\Quasar\Drivers\RemoteDesktop\{version}` (or equivalent on Mono) and validate hashes before installing.
- [ ] `Quasar.Client/RemoteDesktop/Driver/ServiceInstaller.cs`: perform SCM work via `advapi32!CreateService`/`StartService`; fall back to `sc.exe`/`pnputil.exe` when the API fails, and emit structured errors back to `KernelUnblockResult`.
- [ ] `Quasar.Client/Setup/ClientInstaller.cs`: hook into `ApplySettings()` to optionally pre-extract the driver if unattended mode demands it, so remote commands do not have to ship large payloads later.
- [ ] `Quasar.Common.Tests`: add coverage that `DriverPackageReader` validates SHA256 hashes, handles missing files gracefully, and that `KernelDriverManager` selects the newest compatible package.
- [ ] `docs/SECURITY.md` / `README.md`: update disclosure + operational risk sections so operators understand they are shipping an Infinity Hook–style kernel driver.

## Remote Input Unblock Flow

### Background
- Some operators encounter endpoints where keyboard or mouse input stays blocked even after we clear `SetWindowDisplayAffinity`. Common causes: `user32!BlockInput`, malicious hooks consuming events, or being stuck on secure desktops (Winlogon/UAC).
- We want parity with KernelUnblock: server operators should be able to request an input-unblock, track its status, and see logging/telemetry in the UI.
- Any input-unblock feature must be opt-in and transparent because forcing input can interfere with legitimate local sessions or security controls.

### Phase 1 – Protocol + UI scaffolding
- [x] `Quasar.Common/Messages`: add `DoInputUnblock` + `InputUnblockResult` packets and register them with `TypeRegistry`.
- [x] `Quasar.Common/Enums`: define `InputUnblockResultCode` (Success, AlreadyUnlocked, BlockInputFailed, HookRemovalFailed, AccessDenied, Unknown).
- [x] `Quasar.Server/Messages/RemoteDesktopHandler.cs`: send `DoInputUnblock` requests and surface result events on the UI thread.
- [x] `Quasar.Server/Forms/FrmRemoteDesktop.cs/.Designer`: add buttons to trigger input unblocking (mouse/keyboard) and show status to the operator.
- [x] `Quasar.Server/Forms/FrmMain.*`: expose a context-menu entry (matching KernelUnblock) so operators can request unblocking before launching the desktop.
- [x] `docs/remote-desktop-kernel.md`: add a section documenting the new flow, risks, and usage guidance.

### Phase 2 – Client plumbing
- [ ] `Quasar.Client/Messages/RemoteDesktopHandler.cs`: handle `DoInputUnblock`, marshal work off the UI thread, and reply with `InputUnblockResult`.
- [ ] `Quasar.Client/RemoteDesktop/InputUnblockCommand.cs` (new): encapsulate the logic for calling `BlockInput(FALSE)`, detecting secure desktops, and removing low-level hooks; report details back to the server.
- [ ] `Quasar.Client/Helper/NativeMethodsHelper.cs`: wrap `BlockInput`, `AttachThreadInput`, `GetForegroundWindow`, and helper methods for enumerating/removing hooks.
- [ ] Logging: reuse `KernelDriverLogger` (or introduce a shared remote-desktop logger) so every input-unblock attempt is persisted with status/error codes.

### Phase 3 – UX & telemetry
- [ ] Detect when the user is on Winlogon/UAC desktops and return actionable messages rather than blindly calling `BlockInput`.
- [ ] Consider a watchdog that re-enables input if BlockInput is re-applied during an active remote session.
- [ ] Add tooltips/status labels in `FrmRemoteDesktop` so operators know when the client reports “input unblocked” vs “still blocked”.
- [ ] Extend `Quasar.Common.Tests` with round-trip coverage for the new packets and result codes.

### Open questions
- Do we expose per-client policy toggles (e.g., `AllowInputUnblock`) so operators can opt-in only on certain deployments?
- Should the client automatically reset input whenever the operator re-enables mouse/keyboard controls, or only when they explicitly press “Unblock Input”?
- Are there EDR products that will flag `BlockInput(FALSE)` or hook removal, and do we need to provide warnings/logging to operators?

### Open questions / parking lot
- Validate if `Quasar.Client/RemoteDesktop` should own the future DXGI/Desktop Duplication capture path or if it belongs inside `Quasar.CaptureService`.
- Decide whether we keep `KernelUnblock` as a blocking request/response RPC or upgrade to a streaming progress channel (multiple windows per process can take noticeable time).
- Figure out signing requirements for `infinity_hook_pro_max.sys`; do we re-sign it during packaging or expect operators to pre-sign?
- Confirm anti-virus/EDR considerations and whether we need an opt-in flag in `Quasar.Client/Config/Settings` to enable kernel-driver distribution.

## Web Gateway Migration (Rust-first)

### Vision
- Replace the WinForms server with a browser-based control plane: Rust backend (Tokio) + React (or Svelte) SPA.
- Maintain the existing Windows agent in C# initially, then incrementally migrate transport/capture pieces.
- New stack must deliver ultra low latency and be safe for internet exposure (memory safety, TLS, auth).

### Phase 0 – Protocol + Codec Baseline
- [ ] Document every protobuf message, binary blob, and UnsafeStreamCodec detail in `docs/protocol.md`.
- [ ] Decide on media path (keep UnsafeStreamCodec decoded via WebAssembly vs. adopt H.264/WebRTC). Build a reference Rust decoder and verify it can stream to browsers within latency targets.
- [ ] Implement version negotiation + capability flags to allow old/new agents to coexist.

### Phase 1 – Rust Gateway
- [ ] Scaffold a Tokio-based WebSocket server with TLS termination, client registry, and per-client state machine.
- [ ] Port RemoteDesktopHandler logic into Rust services: decode frames, relay input, queue command responses.
- [ ] Introduce REST/WebSocket APIs for other modules (file manager, shell, registry) with auth guards.
- [ ] Back gateway with a database (PostgreSQL) for user accounts, session audit, and client metadata.

### Phase 2 – Web UI
- [ ] Build SPA skeleton (React+TypeScript) with authentication, client list, and per-client dashboards.
- [ ] Implement Remote Desktop canvas: integrate Rust decoder output via WebAssembly or switch to WebRTC streams.
- [ ] Port tooling (file manager, shell, keylogger viewer) into web components, wiring them to the new APIs.
- [ ] Add operator niceties: bandwidth indicators, quality sliders, multi-operator session sharing.

### Phase 3 – Agent Transport Update
- [ ] Abstract transport in `Quasar.Client`: support both legacy TCP and new WebSocket/TLS endpoints.
- [ ] Implement token/JWT-based auth and heartbeat/resume logic compatible with web sessions.
- [ ] Provide feature-negotiation handshake so gateway can request capabilities (kernel unblock, webcam, audio).
- [ ] Optionally introduce a Rust-side helper DLL/service for future capture/input rewrites.

### Phase 4 – Hardening & Ops
- [ ] Integrate OIDC (Keycloak/Auth0) for web login + RBAC per operator/team.
- [ ] Add full audit logging: who accessed which client, file transfers, remote shell commands.
- [ ] Package deployment artifacts (Docker images, Helm chart) with metrics (Prometheus) and alerts.
- [ ] Document disaster recovery, rolling upgrades, and compatibility mode with the WinForms server.

### Phase 5 – Agent Rewrites (optional)
- [ ] Incrementally port high-risk agent modules (capture, input, kernel helpers) to Rust/C++ for better performance.
- [ ] Introduce a Rust “capture service” shared between legacy client and future cross-platform agents.

### Risks / Considerations
- Ensure the Rust gateway can handle GPU-accelerated transcoding (FFmpeg/gstreamer wrappers) for low latency.
- Plan for compliance (logging, data retention) if gateway is internet-facing.
- Provide migration tooling so existing Quasar deployments can run WinForms + web gateway side by side during cutover.
