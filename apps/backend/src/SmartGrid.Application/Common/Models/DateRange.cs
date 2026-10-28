namespace SmartGrid.Application.Common.Models;

/// <summary>
/// An inclusive date range used for filtering queries.
/// Both <see cref="Start"/> and <see cref="End"/> are inclusive boundaries.
/// </summary>
public sealed class DateRange
{
    /// <summary>Inclusive start date.</summary>
    public DateOnly Start { get; }

    /// <summary>Inclusive end date.</summary>
    public DateOnly End { get; }

    public DateRange(DateOnly start, DateOnly end)
    {
        if (start > end)
        {
            throw new ArgumentException(
                $"Start date ({start:yyyy-MM-dd}) must not be later than end date ({end:yyyy-MM-dd}).",
                nameof(start));
        }

        Start = start;
        End = end;
    }
}
