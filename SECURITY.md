# Security Policy

Please report security issues privately and avoid public disclosure until a coordinated fix is available.

## Supported Versions

| Version | Supported          |
|---------|--------------------|
| 0.x     | ✅ Active           |

Only the latest minor release of each major version receives security patches.

## Reporting a Vulnerability

### How to Report

- **Email**: `security@controlagentnet.ai`
- **GitHub**: Use [GitHub Private Vulnerability Reporting](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing/privately-reporting-a-security-vulnerability) on the repository.

### What to Include

Please provide as much of the following as possible:

- **Package name(s)** affected (e.g., `ControlAgentNet.Guards`, `ControlAgentNet.Runtime`)
- **Version(s)** affected
- **Reproduction steps** — minimal code or config to trigger the issue
- **Impact** — what an attacker could achieve
- **Suggested fix** (optional but welcome)

### Response Timeline

| Stage                    | Target       |
|--------------------------|--------------|
| Acknowledgement          | ≤ 48 hours   |
| Initial assessment       | ≤ 5 business days |
| Fix or mitigation        | ≤ 14 days (critical) / 30 days (other) |
| Public disclosure        | After fix is released and users have had time to upgrade |

## Scope

### In-scope

- **Prompt injection bypass** — defeating `PromptInjectionDefenseMiddleware` via crafted input
- **Guard bypass** — circumventing `IToolGuard` evaluation or ordering
- **Policy store tampering** — unauthorized reads/writes to `IToolPolicyStore` or `IChannelPolicyStore`
- **Unauthorized tool execution** — invoking a tool that should be blocked by risk/allowlist guards
- **Human-in-the-loop bypass** — approving tool calls without a legitimate human decision
- **Dependency vulnerabilities** — transitive CVEs in packages directly bundled with ControlAgentNet

### Out-of-scope

- Vulnerabilities in the host application (your ASP.NET Core app, your Telegram bot token management, etc.)
- Issues in third-party dependencies that don't affect ControlAgentNet's security boundary
- Social engineering

## Security Features Overview

ControlAgentNet has several built-in defense layers. Understanding them helps you configure them correctly:

| Feature | Package | Description |
|---------|---------|-------------|
| `PromptInjectionDefenseMiddleware` | `ControlAgentNet.Runtime` | Heuristic-based detection and blocking of role-manipulation attempts in inbound user text. Supports `Off`, `DetectOnly`, and `Block` modes. |
| `RiskDenyGuard` | `ControlAgentNet.Guards` | Denies tool execution if the tool's declared `CapabilityRiskLevel` meets or exceeds a configured threshold. |
| `ToolAllowlistGuard` | `ControlAgentNet.Guards` | Only permits execution of explicitly listed tool IDs. |
| `PolicyEnforcementGuard` | `ControlAgentNet.Guards.Policies` | Enforces per-user/per-channel tool and channel policies stored in `IToolPolicyStore`. |
| `HumanInTheLoop` | `ControlAgentNet.Features.HumanInTheLoop` | Pauses agent execution and requires human approval before high-risk tools execute. |

## Disclosure Policy

We follow **coordinated disclosure**. Once a fix is released:

1. A GitHub Security Advisory will be published.
2. The fix version will be tagged and released on NuGet.
3. The `CHANGELOG.md` entry will reference the advisory.
