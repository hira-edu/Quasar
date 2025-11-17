# Quasar.Driver.RemoteDesktop

This module packages the Infinity Hook driver artifacts used to keep protected windows captureable. The payload (INF/CAT/SYS/PDB) is sourced from `hira-edu/Bypass-SetWindowDisplayAffinity` commit `2adeab8b71b5a65705333cb5bfa457d1c8a4cce7` and stored as `infinity_hook_driver.zip`.

## Contents

```
infinity_hook_driver.zip
├── infinity_hook_pro_max.inf
├── infinity_hook_pro_max.cat
├── infinity_hook_pro_max.sys
└── infinity_hook_pro_max.pdb
```

## Build integration

The companion `Quasar.Driver.RemoteDesktop.csproj` exposes the zip file as an MSBuild item so `Quasar.Client` can embed it as a resource during the build. This keeps the driver payload versioned alongside the source tree and avoids scattering loose binaries across the solution.

If you need to update the driver:

1. Drop the new INF/CAT/SYS/PDB files into a temporary folder.
2. Zip them (without additional nesting) and replace `infinity_hook_driver.zip`.
3. Update this README with the source commit / version.
4. Build `Quasar.Client`; the embedded resource will be updated automatically.

## MSBuild integration

`DriverPackage.targets` exposes a `RemoteDesktopDriverPackage` item that other projects can import to locate the packaged zip along with its source commit hash. `Quasar.Client` currently consumes the zip directly via an `EmbeddedResource`, but the target makes it easy to hook build-time signing or hashing rules later.
