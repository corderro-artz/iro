# Iro

**A lightweight, dependency-free .NET terminal color and markup library.**

Iro renders ANSI truecolor output, falls back to `ConsoleColor` in unsupported environments, and parses a minimal bracket markup syntax — all without any external packages. Every color is an immutable value type, every render operation flows through a single tokenized pipeline, and the library is fully compatible with Native AOT.

[![CI](https://img.shields.io/github/actions/workflow/status/corderro-artz/iro/ci.yml?branch=main&label=CI&logo=githubactions&logoColor=white)](https://github.com/corderro-artz/iro/actions)
[![Release](https://img.shields.io/github/v/tag/corderro-artz/iro?sort=semver&label=Release&logo=git&logoColor=white)](https://github.com/corderro-artz/iro/tags)
[![License: MIT](https://img.shields.io/badge/License-MIT-F7C948?logo=open-source-initiative&logoColor=white)](https://github.com/corderro-artz/iro/blob/main/LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-windows%20%7C%20linux%20%7C%20macOS-lightgrey)](https://github.com/corderro-artz/iro)

## Table of Contents

- [Overview](#overview)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Public API](#public-api)
- [Markup Syntax](#markup-syntax)
- [String Interpolation](#string-interpolation)
- [ANSI Capability Detection](#ansi-capability-detection)
- [Development](#development)
- [Links](#links)
- [Contributing](#contributing)

## Overview

Iro provides a small, focused API for styled terminal output. Input arrives as a markup string, an interpolated string, or a `StyledText` value — all three paths converge on the same internal token pipeline before reaching the renderer.

### Capabilities

| Capability | Details |
| --- | --- |
| Color creation | RGB, ARGB, HEX (`#RGB`, `#RRGGBB`, `#AARRGGBB`), and HSV |
| Style attributes | Foreground color, background color, bold, and underline |
| ANSI rendering | Truecolor escape sequences (`ESC[38;2;R;G;Bm`, `ESC[48;2;R;G;Bm`) |
| ConsoleColor fallback | Nearest-color mapping via minimum squared RGB distance |
| Markup parsing | Named colors, hex colors, nested tags, escaping, and invalid-tag passthrough |
| String interpolation | Custom `[InterpolatedStringHandler]` with optional per-value color format specifiers |
| Capability detection | `NO_COLOR`, `WT_SESSION`, `COLORTERM`, `TERM` heuristics, and redirected-output detection |
| Runtime configuration | `TerminalOptions` for manual ANSI enable/disable and fallback control |
| AOT compatibility | `IsAotCompatible=true` — no reflection, no regex, no dynamic |
| Dependencies | None — .NET BCL only |

## Requirements

| Requirement | Version |
| --- | --- |
| .NET | 10.0 or later |
| Platform | Windows, Linux, macOS |

## Installation

Iro is a single-assembly library. Add a project reference for source consumption or reference `Iro.dll` directly from a build output.

```bash
dotnet add reference path/to/src/Iro/Iro.csproj
```

NuGet packaging is planned for a future release.

## Quick Start

```csharp
using Iro;

// Write markup directly
Terminal.WriteLine("[green]Success[/]");
Terminal.WriteLine("[red]Error:[/] Connection failed");

// Create styled text and pass it anywhere a string is expected
var running = StyledText.Create("Running", fg: Color.Cyan, bold: true);
Console.WriteLine(running);

// Colorize with a factory helper
var warning = Terminal.Colorize("Warning", Color.Yellow);
Console.WriteLine(warning);

// Use string interpolation — literals and values render through the same pipeline
var user = "Alice";
Terminal.WriteLine($"User {user} connected");

// Apply a per-value color via format specifier
var count = 42;
Terminal.WriteLine($"Items: {count:red}");
Terminal.WriteLine($"Status: {"active":#00CC66}");
```

## Architecture

All input — markup strings, interpolated strings, or `StyledText` values — is tokenized into a `StyleToken[]` array before any output is written. The `Renderer` coordinator then selects the appropriate output path based on `TerminalOptions` and the detected environment.

```text
Input
 ↓
MarkupParser  /  TerminalInterpolatedStringHandler
 ↓
StyleToken[]
 ↓
Renderer
 ↓
AnsiRenderer  ──►  ANSI escape sequences  ──►  Console / TextWriter
ConsoleRenderer ►  ConsoleColor fallback   ──►  Console / TextWriter
```

### Design Principles

- Route all rendering through one pipeline — no duplicate implementations.
- Keep the public surface minimal: `Color`, `StyledText`, `Terminal`, `TerminalOptions`.
- Maintain AOT compatibility throughout — no reflection, no regex, no dynamic dispatch.
- Parse in a single O(n) pass using `ReadOnlySpan<char>` and a two-state machine.
- Prefer `readonly record struct` and `ReadOnlySpan<char>` to keep allocations low.

### Internal Structure

```text
src/Iro/
├── Color.cs
├── StyledText.cs
├── Terminal.cs
├── TerminalOptions.cs
├── Interpolation/
│   └── TerminalInterpolatedStringHandler.cs
├── Parsing/
│   └── MarkupParser.cs
├── Rendering/
│   ├── Renderer.cs
│   ├── AnsiRenderer.cs
│   └── ConsoleRenderer.cs
└── Internal/
    ├── Style.cs
    ├── StyleToken.cs
    ├── StyleStack.cs
    ├── TokenType.cs
    ├── ColorConverter.cs
    └── AnsiCapabilityDetector.cs
```

All parser, renderer, token, and conversion types are `internal`. Only the four public types form the library's contract.

## Public API

### `Color`

An immutable `readonly record struct` representing a truecolor value.

```csharp
// Factory methods
Color.FromRgb(byte r, byte g, byte b)
Color.FromArgb(byte a, byte r, byte g, byte b)
Color.FromHex(string hex)     // throws FormatException on invalid input
Color.FromHsv(double hue, double saturation, double value)

// ANSI output
string ToAnsiForeground()     // ESC[38;2;R;G;Bm
string ToAnsiBackground()     // ESC[48;2;R;G;Bm

// Fallback
ConsoleColor ToNearestConsoleColor()
```

Named constants: `Color.Black`, `Color.White`, `Color.Red`, `Color.Green`, `Color.Blue`, `Color.Yellow`, `Color.Cyan`, `Color.Magenta`, `Color.Gray`.

Supported hex formats: `#RGB`, `RGB`, `#RRGGBB`, `RRGGBB`, `#AARRGGBB`, `AARRGGBB`.

### `StyledText`

An immutable `readonly record struct` that holds styled content. `ToString()` renders through the shared pipeline and produces ANSI output when available, or plain text when ANSI is disabled.

```csharp
StyledText.Create(
    string  text,
    Color?  fg        = null,
    Color?  bg        = null,
    bool    bold      = false,
    bool    underline = false)
```

Works transparently with `Console.WriteLine` and string interpolation.

### `Terminal`

The static entry point for all output operations.

```csharp
// Markup-parsed string output
Terminal.Write(string text)
Terminal.WriteLine(string text)

// Styled value output
Terminal.Write(StyledText text)
Terminal.WriteLine(StyledText text)

// Interpolated string output
Terminal.Write($"...")
Terminal.WriteLine($"...")

// Colorize factory
Terminal.Colorize(string text, Color fg)
Terminal.Colorize(string text, Color fg, Color bg)
Terminal.Colorize(string text, Color? fg = null, Color? bg = null, bool bold = false, bool underline = false)

// Markup to StyledText
Terminal.ParseMarkup(string text)

// Global options
Terminal.Options  // TerminalOptions
```

### `TerminalOptions`

Runtime configuration, accessible through `Terminal.Options`.

| Property | Default | Description |
| --- | --- | --- |
| `EnableAnsi` | `true` | Gate for ANSI output. When `DetectAnsiAutomatically` is `false`, this is the sole control. |
| `DetectAnsiAutomatically` | `true` | Combine `EnableAnsi` with environment detection. |
| `EnableConsoleFallback` | `true` | Apply `ConsoleColor` when ANSI is unavailable. Set to `false` for plain-text-only output. |

Manual settings always override automatic detection.

## Markup Syntax

```text
[red]Hello[/]
[#FF8800]colored text[/]
[red]outer [green]inner[/] back to red[/]
```

### Supported Color Tags

Named: `[black]` `[red]` `[green]` `[yellow]` `[blue]` `[magenta]` `[cyan]` `[white]` `[gray]`

Hex: `[#RRGGBB]`

Close: `[/]`

### Escaping

| Input | Output |
| --- | --- |
| `[[` | `[` |
| `]]` | `]` |

### Invalid Tags

Unrecognized tags pass through as literal text. The parser never throws.

```text
[unknown]text    →    [unknown]text
```

## String Interpolation

`Terminal.Write` and `Terminal.WriteLine` accept interpolated strings directly. A custom `[InterpolatedStringHandler]` captures literals and formatted values as style tokens before any string is built.

```csharp
Terminal.WriteLine($"User {username} connected at {time}");
```

### Per-Value Color Format Specifier

Apply a color to an individual interpolated value using a format specifier.

```csharp
Terminal.WriteLine($"Items: {count:red}");
Terminal.WriteLine($"Status: {label:#00CC66}");
```

Supported formats: named colors (`red`, `green`, `yellow`, etc.) and hex strings (`#RRGGBB`, `#RGB`). Unrecognized specifiers render the value as plain text.

## ANSI Capability Detection

Detection runs automatically when `DetectAnsiAutomatically` is `true`. The check order is:

| Condition | Result |
| --- | --- |
| `NO_COLOR` environment variable is set | Disabled |
| `WT_SESSION` environment variable is set | Enabled |
| `COLORTERM` environment variable is set | Enabled |
| `TERM` contains `xterm`, `ansi`, `color`, `vt100`, `screen`, or `tmux` | Enabled |
| Output is redirected (`Console.IsOutputRedirected`) | Disabled |
| None of the above | Disabled |

Set `Terminal.Options.DetectAnsiAutomatically = false` and control `EnableAnsi` directly to override detection entirely.

## Development

```bash
dotnet build
dotnet test
dotnet build src/Iro/Iro.csproj -c Release
```

The Release build produces `Iro.dll` and `Iro.xml` with no third-party dependencies.

### Test Coverage

| Area | Tests |
| --- | --- |
| `Color` — RGB, ARGB, HEX, HSV, ANSI sequences, ConsoleColor mapping | `ColorTests` |
| Markup parsing — tags, nesting, escaping, invalid passthrough | `MarkupParserTests` |
| Rendering — ANSI output, ConsoleColor fallback, style reset, transitions | `RenderingTests` |
| ANSI detection — all environment variable branches | `CapabilityDetectionTests` |
| `StyledText` — creation, rendering, null handling | `StyledTextTests` |
| `Terminal` — all overloads, interpolation, color format specifier | `TerminalTests` |

## Links

| Resource | URL |
| --- | --- |
| Repository | [github.com/corderro-artz/iro](https://github.com/corderro-artz/iro) |
| Specification | [SPECIFICATION.md](SPECIFICATION.md) |
| Issues | [github.com/corderro-artz/iro/issues](https://github.com/corderro-artz/iro/issues) |
| Releases | [github.com/corderro-artz/iro/releases](https://github.com/corderro-artz/iro/releases) |
| License | [LICENSE](LICENSE) |
| Vaporsoft | [vaporsoft.dev](https://www.vaporsoft.dev) |

## Contributing

1. Create a branch from `main` for the change.
2. Run `dotnet build` and `dotnet test` before opening a pull request.
3. Open a pull request with enough context to review the design and behavior impact.

---

Copyright © 2026 [Corderro Artz](https://github.com/corderro-artz) / [Vaporsoft](https://www.vaporsoft.dev).
