using System;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="AnnualRecurrenceOptions" /> class.
/// </summary>
[TestFixture]
public class AnnualRecurrenceOptionsTests
{
	#region StartYear

	[TestCase(Date.MinYear - 1)]
	[TestCase(2001)]
	public void SetStartYear_WithInvalidValue_ThrowsException(Int32 startYear)
	{
		// Arrange:
		AnnualRecurrenceOptions options = new() { EndYear = 2000 };

		// Act:
		Action action = () => options.StartYear = startYear;

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>()
			.WithParameterName("value")
			.WithMessage("The value must be between 1 and 2000 (EndYear).*");
		options.StartYear.Should().Be(Date.MinYear);
	}

	[TestCase(Date.MinYear)]
	[TestCase(2000)]
	public void SetStartYear_WithValidValue_UpdatesProperty(Int32 startYear)
	{
		// Arrange:
		AnnualRecurrenceOptions options = new() { EndYear = 2000 };

		// Act:
		options.StartYear = startYear;

		// Assert:
		options.StartYear.Should().Be(startYear);
	}

	#endregion

	#region EndYear

	[TestCase(1999)]
	[TestCase(Date.MaxYear + 1)]
	public void SetEndYear_WithInvalidValue_ThrowsException(Int32 endYear)
	{
		// Arrange:
		AnnualRecurrenceOptions options = new() { StartYear = 2000 };

		// Act:
		Action action = () => options.EndYear = endYear;

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>()
			.WithParameterName("value")
			.WithMessage("The value must be between 2000 (StartYear) and 9999.*");
		options.EndYear.Should().Be(Date.MaxYear);
	}

	[TestCase(2000)]
	[TestCase(Date.MaxYear)]
	public void SetEndYear_WithValidValue_UpdatesProperty(Int32 endYear)
	{
		// Arrange:
		AnnualRecurrenceOptions options = new() { StartYear = 2000 };

		// Act:
		options.EndYear = endYear;

		// Assert:
		options.EndYear.Should().Be(endYear);
	}

	#endregion
}
