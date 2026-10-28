using SmartGrid.Application.Common.Models;

namespace SmartGrid.Application.Transformers.Queries;

/// <summary>
/// Criteria for searching transformers by metadata.
/// </summary>
public sealed class TransformerSearchCriteria
{
    /// <summary>Optional district filter.</summary>
    public string? District { get; init; }

    /// <summary>Optional governorate filter.</summary>
    public string? Governorate { get; init; }

    /// <summary>Optional feeder ID filter.</summary>
    public string? FeederId { get; init; }

    /// <summary>Optional substation ID filter.</summary>
    public string? SubstationId { get; init; }

    /// <summary>Pagination request.</summary>
    public required PageRequest Pagination { get; init; }
}
