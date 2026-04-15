using Microsoft.Extensions.Options;
using ControlAgentNet.Core.Descriptors;
using ControlAgentNet.Core.Models;
using ControlAgentNet.Guards;
using Xunit;

namespace ControlAgentNet.Guards.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// RiskDenyGuard
// ─────────────────────────────────────────────────────────────────────────────

public class RiskDenyGuardTests
{
    [Theory]
    [InlineData(CapabilityRiskLevel.Low)]
    [InlineData(CapabilityRiskLevel.Medium)]
    public async Task EvaluateAsync_allows_tool_below_threshold(CapabilityRiskLevel riskLevel)
    {
        var guard = CreateRiskDenyGuard(minDenied: CapabilityRiskLevel.High);
        var request = CreateRequest("tool-1", riskLevel);

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Allow, decision.Kind);
    }

    [Theory]
    [InlineData(CapabilityRiskLevel.High)]
    [InlineData(CapabilityRiskLevel.Critical)]
    public async Task EvaluateAsync_denies_tool_at_or_above_threshold(CapabilityRiskLevel riskLevel)
    {
        var guard = CreateRiskDenyGuard(minDenied: CapabilityRiskLevel.High);
        var request = CreateRequest("tool-1", riskLevel);

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Deny, decision.Kind);
        Assert.Contains("risk policy", decision.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EvaluateAsync_allows_exempt_tool_regardless_of_risk()
    {
        var guard = CreateRiskDenyGuard(
            minDenied: CapabilityRiskLevel.High,
            exemptIds: ["dangerous-but-allowed"]);

        var request = CreateRequest("dangerous-but-allowed", CapabilityRiskLevel.Critical);

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Allow, decision.Kind);
    }

    [Fact]
    public async Task EvaluateAsync_exemption_is_case_insensitive()
    {
        var guard = CreateRiskDenyGuard(
            minDenied: CapabilityRiskLevel.High,
            exemptIds: ["MyTool"]);

        var request = CreateRequest("mytool", CapabilityRiskLevel.Critical);

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Allow, decision.Kind);
    }

    [Fact]
    public async Task EvaluateAsync_allows_when_descriptor_is_null()
    {
        var guard = CreateRiskDenyGuard(minDenied: CapabilityRiskLevel.High);
        var request = new ToolExecutionRequest("tool-1", "user-1", "conv-1", Descriptor: null);

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Allow, decision.Kind);
    }

    [Fact]
    public void Order_returns_configured_value()
    {
        var guard = CreateRiskDenyGuard(minDenied: CapabilityRiskLevel.High, order: 42);

        Assert.Equal(42, guard.Order);
    }

    private static RiskDenyGuard CreateRiskDenyGuard(
        CapabilityRiskLevel minDenied,
        ICollection<string>? exemptIds = null,
        int order = 10)
    {
        var options = new RiskDenyGuardOptions
        {
            MinimumDeniedRiskLevel = minDenied,
            ExemptToolIds = exemptIds ?? new List<string>(),
            Order = order
        };
        return new RiskDenyGuard(Options.Create(options));
    }

    private static ToolExecutionRequest CreateRequest(string toolId, CapabilityRiskLevel risk)
        => new(
            toolId,
            "user-1",
            "conv-1",
            Descriptor: new ToolDescriptor(
                Id: toolId,
                Name: toolId,
                Description: "test",
                DefaultEnabled: true,
                Kind: "function",
                Version: "1.0.0",
                RiskLevel: risk,
                SourceAssembly: "Tests"));
}

// ─────────────────────────────────────────────────────────────────────────────
// ToolAllowlistGuard
// ─────────────────────────────────────────────────────────────────────────────

public class ToolAllowlistGuardTests
{
    [Fact]
    public async Task EvaluateAsync_allows_listed_tool()
    {
        var guard = CreateAllowlistGuard(allowedIds: ["greeting"]);
        var request = CreateRequest("greeting");

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Allow, decision.Kind);
    }

    [Fact]
    public async Task EvaluateAsync_denies_unlisted_tool()
    {
        var guard = CreateAllowlistGuard(allowedIds: ["greeting"]);
        var request = CreateRequest("send-email");

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Deny, decision.Kind);
        Assert.Contains("allowlist", decision.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EvaluateAsync_matching_is_case_insensitive()
    {
        var guard = CreateAllowlistGuard(allowedIds: ["Greeting"]);
        var request = CreateRequest("greeting");

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Allow, decision.Kind);
    }

    [Fact]
    public async Task EvaluateAsync_empty_allowlist_denies_all_when_flag_is_false()
    {
        var guard = CreateAllowlistGuard(allowedIds: [], treatEmptyAsAllowAll: false);
        var request = CreateRequest("any-tool");

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Deny, decision.Kind);
    }

    [Fact]
    public async Task EvaluateAsync_empty_allowlist_allows_all_when_flag_is_true()
    {
        var guard = CreateAllowlistGuard(allowedIds: [], treatEmptyAsAllowAll: true);
        var request = CreateRequest("any-tool");

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Allow, decision.Kind);
    }

    [Fact]
    public async Task EvaluateAsync_allows_when_descriptor_is_null()
    {
        var guard = CreateAllowlistGuard(allowedIds: ["greeting"]);
        var request = new ToolExecutionRequest("tool-1", "user-1", "conv-1", Descriptor: null);

        var decision = await guard.EvaluateAsync(request, CancellationToken.None);

        Assert.Equal(ToolGuardDecisionKind.Allow, decision.Kind);
    }

    [Fact]
    public void Order_returns_configured_value()
    {
        var guard = CreateAllowlistGuard(allowedIds: [], order: 55);

        Assert.Equal(55, guard.Order);
    }

    private static ToolAllowlistGuard CreateAllowlistGuard(
        ICollection<string> allowedIds,
        bool treatEmptyAsAllowAll = false,
        int order = 20)
    {
        var options = new ToolAllowlistGuardOptions
        {
            AllowedToolIds = allowedIds,
            TreatEmptyAllowlistAsAllowAll = treatEmptyAsAllowAll,
            Order = order
        };
        return new ToolAllowlistGuard(Options.Create(options));
    }

    private static ToolExecutionRequest CreateRequest(string toolId)
        => new(
            toolId,
            "user-1",
            "conv-1",
            Descriptor: new ToolDescriptor(
                Id: toolId,
                Name: toolId,
                Description: "test",
                DefaultEnabled: true,
                Kind: "function",
                Version: "1.0.0",
                RiskLevel: CapabilityRiskLevel.Low,
                SourceAssembly: "Tests"));
}
