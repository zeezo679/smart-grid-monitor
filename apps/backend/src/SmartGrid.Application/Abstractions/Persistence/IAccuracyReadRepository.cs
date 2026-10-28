using SmartGrid.Application.Accuracy.Models;

namespace SmartGrid.Application.Abstractions.Persistence;

/// <summary>
/// Read-only repository for detection accuracy results.
/// </summary>
/// <remarks>
/// Backed by <c>accuracy_summary</c>.
/// The report is based on the existing warehouse accuracy models.
/// It does not re-implement detection or compare directly against the simulator's answer key.
/// </remarks>
public interface IAccuracyReadRepository
{
    /// <summary>
    /// Retrieves the complete accuracy report with per-type breakdown and overall metrics.
    /// </summary>
    Task<AccuracyReportDto> GetReportAsync(CancellationToken cancellationToken = default);
}
