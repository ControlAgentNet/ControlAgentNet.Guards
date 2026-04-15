using System.Collections.Frozen;
using Microsoft.Extensions.Options;
using ControlAgentNet.Core.Abstractions;
using ControlAgentNet.Core.Models;

namespace ControlAgentNet.Guards;

public sealed class ToolAllowlistGuard : IToolGuard
{
    private readonly ToolAllowlistGuardOptions _options;
    private readonly FrozenSet<string> _allowedToolIds;

    public int Order => _options.Order;

    public ToolAllowlistGuard(IOptions<ToolAllowlistGuardOptions> options)
    {
        _options = options.Value;
        _allowedToolIds = _options.AllowedToolIds?.ToFrozenSet(StringComparer.OrdinalIgnoreCase) 
            ?? FrozenSet<string>.Empty;
    }

    public Task<ToolGuardDecision> EvaluateAsync(ToolExecutionRequest request, CancellationToken cancellationToken)
    {
        if (_allowedToolIds.Count == 0 && _options.TreatEmptyAllowlistAsAllowAll)
        {
            return Task.FromResult(ToolGuardDecision.Allow());
        }

        var descriptor = request.Descriptor;
        if (descriptor == null)
            return Task.FromResult(ToolGuardDecision.Allow());

        var isAllowed = _allowedToolIds.Contains(descriptor.Id) 
            || _allowedToolIds.Contains(descriptor.Name);

        return isAllowed
            ? Task.FromResult(ToolGuardDecision.Allow())
            : Task.FromResult(ToolGuardDecision.Deny(
                $"Tool '{descriptor.Name}' is not part of the configured allowlist."));
    }
}

public sealed class ToolAllowlistGuardOptions
{
    public ICollection<string> AllowedToolIds { get; set; } = new List<string>();
    public bool TreatEmptyAllowlistAsAllowAll { get; set; } = false;
    public int Order { get; set; } = 20;
}
