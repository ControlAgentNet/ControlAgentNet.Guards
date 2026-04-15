using System.Collections.Frozen;
using Microsoft.Extensions.Options;
using ControlAgentNet.Core.Abstractions;
using ControlAgentNet.Core.Descriptors;
using ControlAgentNet.Core.Models;

namespace ControlAgentNet.Guards;

public sealed class RiskDenyGuard : IToolGuard
{
    private readonly RiskDenyGuardOptions _options;
    private readonly FrozenSet<string> _exemptToolIds;

    public int Order => _options.Order;

    public RiskDenyGuard(IOptions<RiskDenyGuardOptions> options)
    {
        _options = options.Value;
        _exemptToolIds = _options.ExemptToolIds?.ToFrozenSet(StringComparer.OrdinalIgnoreCase) 
            ?? FrozenSet<string>.Empty;
    }

    public Task<ToolGuardDecision> EvaluateAsync(ToolExecutionRequest request, CancellationToken cancellationToken)
    {
        var descriptor = request.Descriptor;
        if (descriptor == null)
            return Task.FromResult(ToolGuardDecision.Allow());

        if (_exemptToolIds.Contains(descriptor.Id) 
            || _exemptToolIds.Contains(descriptor.Name))
        {
            return Task.FromResult(ToolGuardDecision.Allow());
        }

        return descriptor.RiskLevel >= _options.MinimumDeniedRiskLevel
            ? Task.FromResult(ToolGuardDecision.Deny(
                $"Tool '{descriptor.Name}' is denied by risk policy ({descriptor.RiskLevel})."))
            : Task.FromResult(ToolGuardDecision.Allow());
    }
}

public sealed class RiskDenyGuardOptions
{
    public CapabilityRiskLevel MinimumDeniedRiskLevel { get; set; } = CapabilityRiskLevel.High;
    public ICollection<string> ExemptToolIds { get; set; } = new List<string>();
    public int Order { get; set; } = 10;
}
