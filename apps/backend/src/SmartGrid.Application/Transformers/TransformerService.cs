using SmartGrid.Application.Abstractions.Persistence;
using SmartGrid.Application.Common.Models;
using SmartGrid.Application.Transformers.Models;
using SmartGrid.Application.Transformers.Queries;

namespace SmartGrid.Application.Transformers;

/// <summary>
/// Application service implementation for transformer inventory searches and lookups.
/// </summary>
public sealed class TransformerService : ITransformerService
{
    private readonly ITransformerReadRepository _repository;

    public TransformerService(ITransformerReadRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc />
    public Task<PagedResult<TransformerListItemDto>> SearchAsync(
        TransformerSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        return _repository.SearchAsync(criteria, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TransformerDetailsDto?> GetByIdAsync(
        string transformerId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transformerId))
        {
            throw new ArgumentException("Transformer ID must not be null or whitespace.", nameof(transformerId));
        }

        return _repository.GetByIdAsync(transformerId, cancellationToken);
    }
}
