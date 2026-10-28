using SmartGrid.Application.Abstractions.Persistence;
using SmartGrid.Application.Common.Models;
using SmartGrid.Application.Transformers.Models;
using SmartGrid.Application.Transformers.Queries;
using SmartGrid.Infrastructure.Data;

namespace SmartGrid.Infrastructure.Repositories;

/// <summary>
/// Mock repository implementation for transformer inventory, metadata, and peaks using deterministic fixtures.
/// </summary>
public sealed class MockTransformerRepository : ITransformerReadRepository
{
    private static readonly Lazy<IReadOnlyDictionary<string, int>> AlertCountsByTransformer = new(() =>
        MockDashboardData.AlertFixtures
            .GroupBy(a => a.TransformerId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase));

    /// <inheritdoc />
    public Task<PagedResult<TransformerListItemDto>> SearchAsync(
        TransformerSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        cancellationToken.ThrowIfCancellationRequested();

        var query = MockGridMapData.Transformers
            .Where(t => MockGridMapData.IsValidCoordinate(t.Latitude, t.Longitude))
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(criteria.District))
        {
            query = query.Where(t => string.Equals(t.District, criteria.District, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Governorate))
        {
            query = query.Where(t => string.Equals(t.Governorate, criteria.Governorate, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.FeederId))
        {
            query = query.Where(t => string.Equals(t.FeederId, criteria.FeederId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.SubstationId))
        {
            query = query.Where(t => string.Equals(t.SubstationId, criteria.SubstationId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            query = query.Where(t => t.TransformerId.Contains(criteria.Search, StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.OnlyWithAlerts)
        {
            query = query.Where(t =>
                AlertCountsByTransformer.Value.TryGetValue(t.TransformerId, out var count) && count > 0);
        }

        var ordered = query.OrderBy(t => t.TransformerId).ToList();
        var totalCount = ordered.Count;

        var pagedItems = ordered
            .Skip((criteria.Pagination.Page - 1) * criteria.Pagination.PageSize)
            .Take(criteria.Pagination.PageSize)
            .Select(t =>
            {
                AlertCountsByTransformer.Value.TryGetValue(t.TransformerId, out var count);
                return new TransformerListItemDto
                {
                    TransformerId = t.TransformerId,
                    District = t.District,
                    Governorate = t.Governorate,
                    FeederId = t.FeederId,
                    SubstationId = t.SubstationId,
                    CapacityKw = t.CapacityKw,
                    Latitude = t.Latitude,
                    Longitude = t.Longitude,
                    PeakKwRecorded = t.PeakKwRecorded,
                    PeakLoadPercentage = t.PeakLoadPercentage,
                    AlertCount = count,
                };
            })
            .ToList();

        return Task.FromResult(new PagedResult<TransformerListItemDto>(
            pagedItems,
            criteria.Pagination.Page,
            criteria.Pagination.PageSize,
            totalCount));
    }

    /// <inheritdoc />
    public Task<TransformerDetailsDto?> GetByIdAsync(
        string transformerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transformerId);
        cancellationToken.ThrowIfCancellationRequested();

        var match = MockGridMapData.Transformers.FirstOrDefault(t =>
            string.Equals(t.TransformerId, transformerId, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            return Task.FromResult<TransformerDetailsDto?>(null);
        }

        var details = new TransformerDetailsDto
        {
            TransformerId = match.TransformerId,
            District = match.District,
            Governorate = match.Governorate,
            FeederId = match.FeederId,
            SubstationId = match.SubstationId,
            CapacityKw = match.CapacityKw,
            Latitude = match.Latitude,
            Longitude = match.Longitude,
        };

        return Task.FromResult<TransformerDetailsDto?>(details);
    }

    /// <inheritdoc />
    public Task<PagedResult<TransformerPeakDto>> GetPeaksAsync(
        TransformerPeakCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var peaks = MockGridMapData.Transformers
            .Where(t => t.PeakKwRecorded.HasValue && t.PeakLoadPercentage.HasValue)
            .Select(t => new TransformerPeakDto(
                t.TransformerId,
                t.District,
                t.CapacityKw,
                t.PeakKwRecorded!.Value,
                t.PeakLoadPercentage!.Value))
            .ToList();

        return Task.FromResult(new PagedResult<TransformerPeakDto>(
            peaks,
            criteria.Pagination.Page,
            criteria.Pagination.PageSize,
            peaks.Count));
    }

    /// <inheritdoc />
    public Task<PagedResult<TransformerReadingDto>> GetReadingsAsync(
        TransformerReadingCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        return Task.FromResult(PagedResult<TransformerReadingDto>.Empty(criteria.Pagination.Page, criteria.Pagination.PageSize));
    }
}
