Here's a project that credibly covers the required bullets. The core idea: a **WPF desktop app that monitors and controls a simulated hardware device over TCP/serial** — because that hits _hardware integration_, _async programming_, _MVVM_, _communication protocols_, and _WPF UI_ all at once, which is basically the exact shape of Quartus's work.

## Project: "SensorDeck" — a hardware telemetry dashboard

A WPF app that connects to a device (real or simulated), streams live sensor readings, plots them, and sends control commands back. You build two pieces: a tiny **device simulator** (console app that speaks TCP/UDP) and the **WPF client**.

This maps to their bullets like this:

| Their requirement                                   | How the project covers it                                     |
| --------------------------------------------------- | ------------------------------------------------------------- |
| .NET, C#, C/C++                                     | Whole thing is C#/.NET; optionally write the simulator in C++ |
| WPF / WinUI                                         | The dashboard UI                                              |
| OOP, MVVM, Producer/Consumer                        | MVVM architecture; Producer/Consumer for the telemetry stream |
| Async/Await, Futures                                | All I/O (socket reads, command sends) is async                |
| Communication protocols (UART, TCP/IP, UDP, MODBUS) | TCP/UDP connection to the device; optional serial             |
| Testing on real-world hardware                      | Simulator stands in; mention you'd swap in real serial        |
| Unit + integration testing                          | Test the parser and view-model logic                          |

## Architecture

**1. Device simulator (separate console project)**
Emits fake telemetry (temperature, voltage, vibration) as JSON or a simple binary frame over a TCP socket every ~100ms. Accepts command messages back (e.g. "set sample rate", "toggle heater"). This is your stand-in for real hardware — and it's honest to say so in an interview.

_Optional flex:_ write this one in C++ so you legitimately have C++ in the project. A ~150-line TCP server that pushes readings is enough.

**2. WPF client (the main app)**

- **Model layer** — a `TelemetryFrame` record, a `DeviceConnection` class wrapping the socket with async read/write.
- **Producer/Consumer** — socket reader (producer) drops frames into a `System.Threading.Channels.Channel<TelemetryFrame>`; a consumer task pulls them and marshals to the UI thread. This is the single most important pattern to nail, because they list it explicitly.
- **ViewModel layer** — `MainViewModel` exposing `ObservableCollection` of readings, `IsConnected`, `CurrentTemp`, and `ICommand`s (`ConnectCommand`, `SendCommand`). Implements `INotifyPropertyChanged`. This _is_ MVVM — no code-behind logic in the view.
- **View layer** — XAML with data bindings only. A live chart (use `LiveCharts2` or `ScottPlot.WPF`), a connection status indicator, a few control buttons/sliders.

## What "done" looks like

A window that: connects to the simulator with one click, shows three live-updating numeric readouts, plots one of them scrolling in real time, and has a button that sends a command the simulator visibly responds to. That's a genuinely presentable demo — screen-record it for applications.

## Testing (don't skip — it's a bullet)

- **Unit tests** (xUnit): frame parser correctly decodes bytes/JSON → `TelemetryFrame`; view-model raises `PropertyChanged`; command enables/disables based on connection state.
- **Integration test**: spin up the simulator in the test fixture, connect, assert frames flow through the channel. This is the one that lets you say "I integration-tested against a device endpoint."

## Suggested build order

1. Simulator emitting TCP telemetry you can see with `telnet`/`netcat`.
2. WPF app that connects and prints raw frames to a `TextBlock` (proves the async socket loop).
3. Refactor into MVVM — move logic into `MainViewModel`, bind the UI.
4. Add the Producer/Consumer channel between socket and UI.
5. Add the live chart.
6. Add outbound commands.
7. Write the tests.

Each step is a working checkpoint, which suits how you build.

## Resume line it earns you

Something like: _"Built a WPF/.NET telemetry dashboard integrating with a networked device over TCP, using MVVM and a Producer/Consumer pipeline for real-time async data streaming; unit- and integration-tested with xUnit."_ — every noun in that sentence is one of their bullets.

One thing worth flagging: this overlaps conceptually with Sentinel-Grid (sensor telemetry). That's good — you can talk about the _domain_ fluently — but keep this a clean, separate, obviously-C# repo so the .NET skill reads unambiguously.

Want me to write out the actual starter code — the simulator plus the MVVM skeleton with the async socket loop and channel wired up?
