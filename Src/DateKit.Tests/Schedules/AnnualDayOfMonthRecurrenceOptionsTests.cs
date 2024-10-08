using System;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="AnnualDayOfMonthRecurrenceOptions" /> class.
/// </summary>
[TestFixture]
public class AnnualDayOfMonthRecurrenceOptionsTests
{
	#region StartDate

	[TestCase(Date.MinYear - 1)]
	[TestCase(2001)]
	public void SetStartDate_WithInvalidValue_ThrowsException(Int32 startYear)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options = new() { EndYear = 2000 };

		// Act:
		Action action = () => options.StartYear = startYear;

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>()
			.WithParameterName("value")
			.WithMessage("StartYear must be between 1 and 2000 (the value of EndYear).*");
	}

	[TestCase(Date.MinYear)]
	[TestCase(2000)]
	public void SetStartDate_WithValidValue_UpdatesProperty(Int32 startYear)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options = new() { EndYear = 2000 };

		// Act:
		options.StartYear = startYear;

		// Assert:
		options.StartYear.Should().Be(startYear);
	}

	#endregion

	#region EndDate

	[TestCase(1999)]
	[TestCase(Date.MaxYear + 1)]
	public void SetEndDate_WithInvalidValue_ThrowsException(Int32 endYear)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options = new() { StartYear = 2000 };

		// Act:
		Action action = () => options.EndYear = endYear;

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>()
			.WithParameterName("value")
			.WithMessage("EndYear must be between 2000 (the value of StartYear) and 9999.*");
	}

	[TestCase(2000)]
	[TestCase(Date.MaxYear)]
	public void SetEndDate_WithValidValue_UpdatesProperty(Int32 endYear)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options = new() { StartYear = 2000 };

		// Act:
		options.EndYear = endYear;

		// Assert:
		options.EndYear.Should().Be(endYear);
	}

	#endregion
}
