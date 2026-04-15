using Microsoft.Extensions.DependencyInjection;
using ControlAgentNet.Core.Abstractions;
using ControlAgentNet.Core.Descriptors;
using ControlAgentNet.Core.Models;
using ControlAgentNet.Guards;

Console.WriteLine("=== ControlAgentNet.Guards Demo ===\n");

var services = new ServiceCollection();

services.AddRiskDenyGuard(opts =>
{
    opts.MinimumDeniedRiskLevel = CapabilityRiskLevel.Medium;
    opts.ExemptToolIds = new[] { "safe_tool", "greeting" };
    opts.Order = 10;
});

services.AddToolAllowlistGuard(opts =>
{
    opts.AllowedToolIds = new[] { "greeting", "safe_tool", "search" };
    opts.TreatEmptyAllowlistAsAllowAll = false;
    opts.Order = 20;
});

var provider = services.BuildServiceProvider();
var guards = provider.GetServices<IToolGuard>().OrderBy(g => g.Order).ToList();

Console.WriteLine("Registered Guards (in order):");
foreach (var guard in guards)
{
    Console.WriteLine($"  - {guard.GetType().Name} (Order: {guard.Order})");
}

Console.WriteLine("\n--- Testing RiskDenyGuard ---\n");

var riskTests = new[]
{
    ("greeting", "Safe tool (Low risk - exempt)", CapabilityRiskLevel.Low),
    ("safe_tool", "Safe tool (Low risk)", CapabilityRiskLevel.Low),
    ("medium_risk", "Medium risk tool", CapabilityRiskLevel.Medium),
    ("high_risk", "High risk tool", CapabilityRiskLevel.High),
};

foreach (var (toolId, description, riskLevel) in riskTests)
{
    var descriptor = new ToolDescriptor(
        Id: toolId,
        Name: toolId,
        Description: description,
        DefaultEnabled: true,
        Kind: "test",
        Version: "1.0.0",
        RiskLevel: riskLevel,
        SourceAssembly: "test",
        Category: "test");

    var request = new ToolExecutionRequest(toolId, "user-1", "conv-1")
    {
        Descriptor = descriptor
    };

    var decision = await guards[0].EvaluateAsync(request);
    Console.WriteLine($"Tool: {toolId} ({description})");
    Console.WriteLine($"  Risk Level: {riskLevel}");
    Console.WriteLine($"  Decision: {decision.Kind}");
    if (decision.Reason != null)
        Console.WriteLine($"  Reason: {decision.Reason}");
    Console.WriteLine();
}

Console.WriteLine("--- Testing ToolAllowlistGuard ---\n");

var allowlistTests = new[]
{
    ("greeting", "In allowlist"),
    ("safe_tool", "In allowlist"),
    ("search", "In allowlist"),
    ("admin_tool", "NOT in allowlist"),
    ("delete_data", "NOT in allowlist"),
};

foreach (var (toolId, description) in allowlistTests)
{
    var descriptor = new ToolDescriptor(
        Id: toolId,
        Name: toolId,
        Description: description,
        DefaultEnabled: true,
        Kind: "test",
        Version: "1.0.0",
        RiskLevel: CapabilityRiskLevel.Low,
        SourceAssembly: "test",
        Category: "test");

    var request = new ToolExecutionRequest(toolId, "user-1", "conv-1")
    {
        Descriptor = descriptor
    };

    var decision = await guards[1].EvaluateAsync(request);
    Console.WriteLine($"Tool: {toolId} ({description})");
    Console.WriteLine($"  Decision: {decision.Kind}");
    if (decision.Reason != null)
        Console.WriteLine($"  Reason: {decision.Reason}");
    Console.WriteLine();
}

Console.WriteLine("--- Testing Combined Guards ---\n");

var combinedTests = new[]
{
    ("greeting", "Low risk, in allowlist", CapabilityRiskLevel.Low),
    ("admin_tool", "Low risk, NOT in allowlist", CapabilityRiskLevel.Low),
    ("medium_risk", "Medium risk, in allowlist", CapabilityRiskLevel.Medium),
    ("high_risk", "High risk, NOT in allowlist", CapabilityRiskLevel.High),
};

foreach (var (toolId, description, riskLevel) in combinedTests)
{
    var descriptor = new ToolDescriptor(
        Id: toolId,
        Name: toolId,
        Description: description,
        DefaultEnabled: true,
        Kind: "test",
        Version: "1.0.0",
        RiskLevel: riskLevel,
        SourceAssembly: "test",
        Category: "test");

    var request = new ToolExecutionRequest(toolId, "user-1", "conv-1")
    {
        Descriptor = descriptor
    };

    Console.WriteLine($"Tool: {toolId} ({description})");

    foreach (var guard in guards)
    {
        var decision = await guard.EvaluateAsync(request);
        Console.WriteLine($"  {guard.GetType().Name}: {decision.Kind}");
        if (decision.Kind == ToolGuardDecisionKind.Deny && decision.Reason != null)
            Console.WriteLine($"    → {decision.Reason}");
    }
    Console.WriteLine();
}

Console.WriteLine("=== Demo Complete ===");
Console.WriteLine();
Console.WriteLine("Key Points:");
Console.WriteLine("1. Guards execute in Order (RiskDenyGuard=10, ToolAllowlistGuard=20)");
Console.WriteLine("2. RiskDenyGuard blocks Medium+ risk (unless exempt)");
Console.WriteLine("3. ToolAllowlistGuard restricts to specific tools");
Console.WriteLine("4. Both guards can be combined for defense in depth");
