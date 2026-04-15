# Contributing to ControlAgentNet.Guards

This repository contains the tool execution guard implementations for ControlAgentNet: `RiskDenyGuard`, `ToolAllowlistGuard`, and the base `IToolGuard` pipeline.

## Principles

- Guards must be **stateless or thread-safe** — they are registered as singletons.
- Guards must reference only `ControlAgentNet.Core` — not Runtime, not Channels, not Providers.
- Each guard must implement `IToolGuard` and expose a corresponding `*Options` class and `Add*Guard()` extension method.
- Use `FrozenSet` for look-up collections initialized at startup.
- Guard decisions must never throw — return `ToolGuardDecision.Deny(...)` for rejections.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (preview)

## Build

```bash
dotnet restore ControlAgentNet.Guards.slnx
dotnet build ControlAgentNet.Guards.slnx -c Release
```

## Run Tests

```bash
dotnet test ControlAgentNet.Guards.slnx
```

---

## Adding a New Guard

1. Create `MyGuard.cs` in the project root. Implement `IToolGuard`.
2. Create `MyGuardOptions.cs` if configuration is needed.
3. Add an `Add*Guard()` extension on `IServiceCollection` in `GuardExtensions.cs`.
4. Write tests in `samples/ControlAgentNet.Guards.Tests/GuardTests.cs`.
5. Document the guard in `README.md`.

### Guard template

```csharp
public sealed class MyGuard : IToolGuard
{
    private readonly MyGuardOptions _options;
    public int Order => _options.Order;

    public MyGuard(IOptions<MyGuardOptions> options) => _options = options.Value;

    public Task<ToolGuardDecision> EvaluateAsync(ToolExecutionRequest request, CancellationToken ct)
    {
        if (request.Descriptor is null)
            return Task.FromResult(ToolGuardDecision.Allow());

        // ... your logic ...
        return Task.FromResult(ToolGuardDecision.Allow());
    }
}

public sealed class MyGuardOptions
{
    public int Order { get; set; } = 30;
}
```

---

## Branch Conventions

| Branch pattern   | Purpose                              |
|------------------|--------------------------------------|
| `main`           | Stable, always builds, tests pass    |
| `feature/<name>` | New guard or capability              |
| `fix/<name>`     | Bug fixes                            |
| `docs/<name>`    | Documentation-only changes           |

## Pull Request Process

1. Fork and create a branch from `main`.
2. Write tests — every guard behavior must have a test.
3. Run `dotnet test` locally before opening a PR.
4. Open a PR against `main` describing what changed and why.
