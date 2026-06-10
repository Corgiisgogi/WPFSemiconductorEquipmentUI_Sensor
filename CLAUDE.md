# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

WPF desktop UI for a semiconductor-equipment sensor trainer. It polls a Beckhoff
TwinCAT ADS device (analog + digital I/O), evaluates risk thresholds, drives the
equipment's running/warning lamps, persists everything to a local SQLite database,
and mirrors telemetry plus auth to a Flask backend. UI text and many comments are
in Korean.

- **Target:** .NET Framework 4.7.2, WPF, C# (old `csproj` format, `packages.config`).
- **Pattern:** MVVM, hand-rolled (no MVVM framework). `INotifyPropertyChanged` via
  `ViewModelBase`/`ScreenViewModelBase`; commands via `RelayCommand`. No DI
  container — dependencies are constructed and wired by hand in `MainViewModel`'s
  constructor (the composition root).

## Build & Run

This is a Windows-only .NET Framework app; it builds with MSBuild/Visual Studio,
not `dotnet build`.

```powershell
# Restore NuGet packages (System.Data.SQLite). nuget.exe or VS restore.
nuget restore WPFSemiconductorEquipmentUI_Sensor.sln

# Build (use the VS Developer Prompt or full-path msbuild, not `dotnet`)
msbuild WPFSemiconductorEquipmentUI_Sensor.sln /p:Configuration=Debug

# Run
WPFSemiconductorEquipmentUI_Sensor\bin\Debug\WPFSemiconductorEquipmentUI_Sensor.exe
```

There is **no test project**. The `bin\Verify*` folders are ad-hoc throwaway
console harnesses from past manual verification, not a maintained test suite.

### Flask backend (auth + telemetry sink)

The WPF app talks to a Flask server for login/registration/approval and posts
sensor snapshots + activity logs to it. A minimal reference server lives in
`flask_test_server/`:

```powershell
cd flask_test_server
python -m venv .venv; .\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
python app.py          # serves http://localhost:5000, admin UI at /admin
```

Default seeded accounts: `operator01`/`1234` (Approved Operator),
`admin`/`admin1234` (Approved Admin), `pending01`/`1234` (Pending).
The WPF app's target URL is configurable at runtime in the Settings screen
(`ApiBaseUrl`, default `http://localhost:5000`).

## Architecture

### Layers (`WPFSemiconductorEquipmentUI_Sensor/`)
- **Views/** + **MainWindow.xaml** — XAML screens. `MainWindow` sets
  `DataContext = new MainViewModel()`; the active screen is swapped via
  `MainViewModel.CurrentViewModel` + DataTemplates.
- **ViewModels/** — screen logic. `ConsoleViewModel` is the heart of the app
  (sensor polling loop, risk evaluation, lamp control).
- **Services/** — hardware, persistence, and network. Interface-first
  (`ITrainerClient`, `IAuthService`, `IRemoteTelemetryService`) so VMs can run
  with stubs/sample data in the designer.
- **Models/** — plain data records (snapshots, log items, auth results).
- **Controls/** — reusable `SensorCard`, `StatusBadge`.
- **Resources/** — `Colors.xaml`, `Typography.xaml`, `Controls.xaml` merged
  globally in `App.xaml`. Use these styles/brushes rather than inline values.

### Composition root
`MainViewModel()` (`ViewModels/MainViewModel.cs`) constructs the whole object
graph: `DatabaseService` → repositories → stores → services → child VMs. When
adding a service or VM dependency, wire it here. `MainViewModel` owns navigation
(`NavigationItems`), the status-bar clock, and propagates `ConsoleViewModel`'s
risk state up to the global alert badge. It is `IDisposable` and disposes child
VMs on window close.

### Hardware: TwinCAT ADS (`AdsSensorTrainerClient`)
- Talks to the trainer over ADS port **851** using `TwinCAT.Ads.dll`
  (`Lib/TwinCAT.Ads.dll`, copied local). Each read/write opens a short-lived
  `TcAdsClient`.
- Symbols: `GVL.NX_AD4203` (16 bytes of analog: pressure/vibration/temp/humidity
  as `Int16` raw), `GVL.NX_ID5342` (digital inputs, bitfield),
  `GVL.NX_OD5121` (digital outputs / lamps, bitfield).
- **Digital-output bit map is a hard contract:** bit 0 = running lamp,
  bit 1 = process lamp, bit 2 = warning lamp, bit 3 = AI-control lamp. Bit 0 is
  reserved — `PulseDigitalOutput` rejects it.
- `ITrainerClient` lets `ConsoleViewModel` be tested/previewed without hardware.

### Risk engine (`ConsoleViewModel`)
A `DispatcherTimer` polls the trainer (~1 Hz), converts raw values, compares
against `AppSettingsStore` thresholds (pressure/temp/vibration/humidity), and
tracks warnings inside a sliding `RiskWindowSeconds` window. Once
`AutoShutdownWarningLimit` warnings accumulate it auto-shuts-down and drives the
warning lamp. Each poll persists a `SensorSnapshotRecord` (throttled by
`SensorSnapshotSaveIntervalSeconds`) and posts it to Flask. `StaleReadThreshold`
detects an unchanging/stale device.

### Persistence (local SQLite)
- `DatabaseService` creates/owns the DB at
  `%LocalAppData%\WPFSemiconductorEquipmentUI_Sensor\equipment.db` (WAL mode) and
  creates tables on `Initialize()`: `activity_logs`, `sensor_snapshots`,
  `app_settings`. **This is the runtime DB** — the `company_auth.sqlite` /
  `flask_test_server/auth_test.db` files in the repo belong to the Flask side.
- Repository → Store layering: `*Repository` does raw SQLite; `*Store`
  (`ActivityLogStore`, `AppSettingsStore`) adds caching, change notification, and
  fan-out (e.g. `ActivityLogStore` also pushes to the remote telemetry service).
- `AppSettingsStore` holds all tunable thresholds as `Default*` constants and is
  the single source of truth for both the risk engine and the Settings screen.

### Networking (`FlaskAuthService`, `FlaskTelemetryService`)
Plain `HttpWebRequest` + `JavaScriptSerializer` (no HttpClient/JSON.NET).
Auth is synchronous with a 5 s timeout; telemetry posts are fire-and-forget on a
background thread and self-throttled to ~1/sec so polling never blocks the UI.
Error messages surfaced to the user are Korean.

### Auth & navigation gating
`UserSession` holds the logged-in user/role. Settings normally requires Admin,
but `MainViewModel.AllowSettingsWithoutAdminForApiSetup` (currently `true`) is a
**dev/demo switch** that unlocks Settings pre-login so the Flask URL can be set on
a classroom machine. Set it to `false` for real operation.

## Conventions
- Old-style `csproj`: **new `.cs` files are not auto-included** — add a
  `<Compile Include="..." />` entry to `WPFSemiconductorEquipmentUI_Sensor.csproj`
  when creating a file.
- **Status display — two patterns coexist (use the right one):**
  - *Legacy (most of the app):* ViewModels expose paired `*Text` / `*Tone` string
    properties; `*Tone` ("Normal"/"Warning"/"Danger"/"Blue"/"Disabled") is bound to
    the reusable `StatusBadge` control, which maps it to colors in its code-behind.
    These existing `*Tone`/`*Text` props (lamps, FOUP, DI, connection, risk, auth,
    etc.) are intentionally kept as-is — do **not** bulk-convert them.
  - *Preferred for new sensor/numeric status (reference: the sensor cells):* expose a
    **domain enum** on the model (`Models/SensorStatus.cs`) and let the View map it
    with converters in `Converters/` (`SensorStatusToToneConverter`,
    `SensorStatusToBadgeTextConverter`, `SensorValueConverter`) — no display strings,
    Korean text, or formatting in the ViewModel. See `Models/SensorMetric.cs` +
    `Views/ConsoleView.xaml` (`Sensors[i]` bindings). Apply this pattern only when
    adding/reworking such a display, not as a global migration.
- Each VM has a parameterless/sample constructor chain feeding `SampleData` so the
  XAML designer renders without a DB or hardware — preserve these overloads.
