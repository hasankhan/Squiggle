# Squiggle

## Build

Squiggle targets **.NET Framework 4.5.1** and uses classic MSBuild (not `dotnet` CLI). Build with Visual Studio or:

```powershell
msbuild Squiggle.sln /p:Configuration=Release
```

Full release build (compiles, injects git hash, builds WiX installer, packages zip/msi):

```powershell
.\Build\Build.ps1
```

There are no automated tests in this repository.

## Architecture

Squiggle is a **peer-to-peer LAN instant messenger** built with WPF. There is no central server — peers discover each other via multicast and communicate directly.

### Layer diagram

```
Squiggle.UI          (WPF app — windows, viewmodels, plugin loader)
    ↓
Squiggle.Client      (Facade — ChatClient exposes buddy list, login, chat events)
    ↓
Squiggle.Core        (Networking — presence discovery, chat transport, message serialization)
    ↓
Squiggle.Infrastructure  (Shared abstractions — async pipes, serialization helpers)
Squiggle.Utilities       (Cross-cutting helpers used by all layers)
```

### Feature modules (plug into Client/UI)

| Project | Purpose | Key dependency |
|---|---|---|
| `Squiggle.FileTransfer` | P2P file transfer | Squiggle.Client |
| `Squiggle.VoiceChat` | Voice chat via NAudio + Speex | Squiggle.Client, NAudio |
| `Squiggle.Screenshot` | Screen capture & send | Squiggle.FileTransfer |
| `Squiggle.Translate` | Message translation | Squiggle.Client, Newtonsoft.Json |
| `Squiggle.History` | Chat history persistence | EntityFramework 6, System.Data.SQLite |
| `Squiggle.Multicast` | Multicast presence (standalone exe) | Squiggle.Core |
| `Squiggle.Bridge` | Cross-subnet/WAN bridging (standalone exe) | Squiggle.Core, protobuf-net |
| `Squiggle.Setup` | WiX MSI installer | WiX Toolset |

### Networking

- **Presence/discovery**: Multicast UDP (`UdpMulticastService`) or TCP fallback (`TcpMulticastService`) — peers announce themselves and listen for others on the LAN.
- **Chat transport**: Unicast message pipes between peers. `ChatHost` deserializes incoming messages and raises typed events; `ChatService` creates and manages chat sessions.
- **Serialization**: protobuf-net for wire protocol messages.
- **Bridge**: A standalone exe that forwards presence/chat messages between subnets for cross-LAN communication.

### UI patterns

- WPF with MVVM-style bindings. `MainWindow` uses a `ClientViewModel` and command pattern.
- Plugin/extension system: `Squiggle.Core` defines `IExtension`, `IMessageFilter`, `IMessageParser` interfaces; `PluginLoader` in the UI discovers and loads plugins at startup.
- Entry point is `Squiggle.UI/App.xaml.cs` which enforces single-instance via a bootstrapper.

## Conventions

- Events are initialized with empty delegates (`event ... = delegate { };`) to avoid null checks.
- Diagnostic logging uses `System.Diagnostics.Trace.WriteLine` throughout.
- Third-party DLLs are committed in `Libraries/` (NAudio, Speex, FluidKit) — not NuGet-managed.
- Translations live in `Translations/` as `.resx` satellite assemblies.
