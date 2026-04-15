# ControlAgentNet.Guards

<p align="center">
  <img src="https://img.shields.io/github/license/ControlAgentNet/ControlAgentNet.Guards" alt="License">
  <img src="https://img.shields.io/github/actions/workflow/status/ControlAgentNet/ControlAgentNet.Guards/ci.yml?branch=main" alt="CI">
  <img src="https://img.shields.io/nuget/v/ControlAgentNet.Guards" alt="NuGet Version">
</p>

> Tool execution guards for ControlAgentNet agents.

## Features

- `RiskDenyGuard` - Block tools by risk level
- `ToolAllowlistGuard` - Only allow specific tools
- Configurable order for guard execution
- Exemption support for risk-based guards
- FrozenSet for efficient lookups
- Zero external dependencies beyond Microsoft.Extensions.Options
- Composable and extensible

## Installation

```bash
dotnet add package ControlAgentNet.Guards
```

## RiskDenyGuard

Blocks tools that exceed a configurable risk threshold.

```csharp
using ControlAgentNet.Guards;
using ControlAgentNet.Core.Descriptors;

builder.Services.AddRiskDenyGuard(opts =>
{
    opts.MinimumDeniedRiskLevel = CapabilityRiskLevel.High;
    opts.ExemptToolIds = new[] { "admin_tool", "safe_tool" };
    opts.Order = 10;
});
```

### Options

| Property | Default | Description |
|----------|---------|-------------|
| `MinimumDeniedRiskLevel` | High | Minimum risk level to block |
| `ExemptToolIds` | empty | Tools to exempt from blocking |
| `Order` | 10 | Execution order |

## ToolAllowlistGuard

Only allows specific tools to execute.

```csharp
using ControlAgentNet.Guards;

builder.Services.AddToolAllowlistGuard(opts =>
{
    opts.AllowedToolIds = new[] { "greeting", "search", "weather" };
    opts.TreatEmptyAllowlistAsAllowAll = false;
    opts.Order = 20;
});
```

### Options

| Property | Default | Description |
|----------|---------|-------------|
| `AllowedToolIds` | empty | List of allowed tool IDs |
| `TreatEmptyAllowlistAsAllowAll` | false | Allow all if list is empty |
| `Order` | 20 | Execution order |

## Custom Guards

```csharp
using ControlAgentNet.Core.Abstractions;
using ControlAgentNet.Core.Models;

public class RoleGuard : IToolGuard
{
    public int Order => 5;

    public Task<ToolGuardDecision> EvaluateAsync(
        ToolExecutionRequest request, 
        CancellationToken ct)
    {
        if (!user.IsAdmin)
            return Task.FromResult(ToolGuardDecision.Deny("Admin only"));
        return Task.FromResult(ToolGuardDecision.Allow());
    }

    public Task<bool> CanExecuteAsync(
        string toolName, 
        IDictionary<string, object?>? arguments, 
        string conversationId, 
        string userId, 
        CancellationToken ct)
        => Task.FromResult(true);

    public string GetDenialReason(string toolName) 
        => $"Tool '{toolName}' requires admin role.";
}

builder.Services.AddToolGuard<RoleGuard>();
```

## Build

```bash
dotnet restore ControlAgentNet.Guards.slnx
dotnet build ControlAgentNet.Guards.slnx -c Release
dotnet test ControlAgentNet.Guards.slnx -c Release --no-build
dotnet pack ControlAgentNet.Guards.slnx -c Release -o artifacts/nuget
```

## Versioning

- local builds: `0.1.5-dev`
- pull requests: `0.1.5-preview.<run_number>`
- pushes to `main`: `0.1.5-alpha.<run_number>`
- tags like `v0.1.5`: exact stable package version `0.1.5`

See `VERSIONING.md` for the release flow.
