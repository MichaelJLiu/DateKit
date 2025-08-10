using System;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="DaysOfWeekRecurrenceOptions" /> class.
/// </summary>
[TestFixture]
public class DaysOfWeekRecurrenceOptionsTests
{
	#region StartDate

	[Test]
	public void SetStartDate_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		DaysOfWeekRecurrenceOptions options = new();
		Date startDate = Date.Empty;

		// Act:
		Action action = () => options.StartDate = startDate;

		// Assert:
		action.Should().Throw<ArgumentException>()
			.WithParameterName("value")
			.WithMessage("The value cannot be the default (empty) date.*");
		options.StartDate.Should().Be(Date.MinValue);
	}

	[TestCase("2000-06-10")]
	[TestCase("9999-12-31")]
	public void SetStartDate_WithInvalidValue_ThrowsException(Date startDate)
	{
		// Arrange:
		DaysOfWeekRecurrenceOptions options = new() { EndDate = new Date(2000, 6, 15) };

		// Act:
		Action action = () => options.StartDate = startDate;

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>()
			.WithParameterName("value")
			.WithMessage("The value cannot be later than 2000-06-09 (six days before EndDate).*");
		options.StartDate.Should().Be(Date.MinValue);
	}

	[TestCase("0001-01-01")]
	[TestCase("2000-06-09")]
	public void SetStartDate_WithValidValue_UpdatesProperty(Date startDate)
	{
		// Arrange:
		DaysOfWeekRecurrenceOptions options = new() { EndDate = new Date(2000, 6, 15) };

		// Act:
		options.StartDate = startDate;

		// Assert:
		options.StartDate.Should().Be(startDate);
	}

	#endregion

	#region EndDate

	[Test]
	public void SetEndDate_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		DaysOfWeekRecurrenceOptions options = new();
		Date endDate = Date.Empty;

		// Act:
		Action action = () => options.EndDate = endDate;

		// Assert:
		action.Should().Throw<ArgumentException>()
			.WithParameterName("value")
			.WithMessage("The value cannot be the default (empty) date.*");
		options.EndDate.Should().Be(Date.MaxValue);
	}

	[TestCase("0001-01-01")]
	[TestCase("2000-06-20")]
	public void SetEndDate_WithInvalidValue_ThrowsException(Date endDate)
	{
		// Arrange:
		DaysOfWeekRecurrenceOptions options = new() { StartDate = new Date(2000, 6, 15) };

		// Act:
		Action action = () => options.EndDate = endDate;

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>()
			.WithParameterName("value")
			.WithMessage("The value cannot be earlier than 2000-06-21 (six days after StartDate).*");
		options.EndDate.Should().Be(Date.MaxValue);
	}

	[TestCase("2000-06-21")]
	[TestCase("9999-12-31")]
	public void SetEndDate_WithValidValue_UpdatesProperty(Date endDate)
	{
		// Arrange:
		DaysOfWeekRecurrenceOptions options = new() { StartDate = new Date(2000, 6, 15) };

		// Act:
		options.EndDate = endDate;

		// Assert:
		options.EndDate.Should().Be(endDate);
	}

	#endregion
}
