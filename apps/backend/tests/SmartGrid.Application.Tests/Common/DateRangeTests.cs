using SmartGrid.Application.Common.Models;

namespace SmartGrid.Application.Tests.Common;

public class DateRangeTests
{
    [Fact]
    public void Constructor_WithValidRange_SetsProperties()
    {
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 12, 31);

        var range = new DateRange(start, end);

        Assert.Equal(start, range.Start);
        Assert.Equal(end, range.End);
    }

    [Fact]
    public void Constructor_WithSameStartAndEnd_IsValid()
    {
        var date = new DateOnly(2024, 6, 15);

        var range = new DateRange(date, date);

        Assert.Equal(date, range.Start);
        Assert.Equal(date, range.End);
    }

    [Fact]
    public void Constructor_WithStartAfterEnd_ThrowsArgumentException()
    {
        var start = new DateOnly(2024, 12, 31);
        var end = new DateOnly(2024, 1, 1);

        var ex = Assert.Throws<ArgumentException>(() => new DateRange(start, end));
        Assert.Contains("must not be later than", ex.Message);
    }
}
