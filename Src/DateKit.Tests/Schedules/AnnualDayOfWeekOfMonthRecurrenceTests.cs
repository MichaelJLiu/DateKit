using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="AnnualDayOfWeekOfMonthRecurrence" /> class.
/// </summary>
[TestFixture]
public class AnnualDayOfWeekOfMonthRecurrenceTests
{
	[TestCase(0, DayOfWeek.Sunday, -4, "month")]
	[TestCase(1, (DayOfWeek)(-1), -1, "dayOfWeek")]
	[TestCase(3, DayOfWeek.Monday, -5, "occurrence")]
	[TestCase(6, DayOfWeek.Wednesday, 0, "occurrence")]
	[TestCase(9, DayOfWeek.Thursday, 5, "occurrence")]
	[TestCase(12, (DayOfWeek)7, 1, "dayOfWeek")]
	[TestCase(13, DayOfWeek.Saturday, 4, "month")]
	public void Constructor_WithInvalidArguments_ThrowsException(
		Int32 month, DayOfWeek dayOfWeek, Int32 occurrence, String expectedParamName)
	{
		// Act:
		Func<AnnualDayOfWeekOfMonthRecurrence> func = () =>
			new AnnualDayOfWeekOfMonthRecurrence(month, dayOfWeek, occurrence);

		// Assert:
		func.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(expectedParamName);
	}

	[TestCase(1, DayOfWeek.Sunday, -4)]
	[TestCase(2, DayOfWeek.Monday, -1)]
	[TestCase(11, DayOfWeek.Friday, 1)]
	[TestCase(12, DayOfWeek.Saturday, 4)]
	public void Constructor_WithValidArguments_ReturnsInstance(
		Int32 month, DayOfWeek dayOfWeek, Int32 occurrence)
	{
		// Act:
		AnnualDayOfWeekOfMonthRecurrence recurrence = new(month, dayOfWeek, occurrence);

		// Assert:
		recurrence.Month.Should().Be(month);
		recurrence.DayOfWeek.Should().Be(dayOfWeek);
		recurrence.Occurrence.Should().Be(occurrence);
	}

	[Test]
	public void Contains_WithEmptyDate_ReturnsFalse()
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(6, DayOfWeek.Thursday, 3);
		Date date = Date.Empty;

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().BeFalse();
	}

	[TestCase(3, 1999, 4, 15, false)] // wrong month
	[TestCase(3, 2000, 6, 14, false)] // wrong day of week
	[TestCase(3, 2000, 6, 15, true)] // right occurrence; maxDay - date.Day == 6
	[TestCase(3, 2000, 6, 22, false)] // wrong occurrence; maxDay - date.Day == -1
	[TestCase(3, 2001, 6, 14, false)] // wrong occurrence; maxDay - date.Day == 7
	[TestCase(3, 2001, 6, 21, true)] // right occurrence; maxDay - date.Day == 0
	[TestCase(-3, 2004, 6, 10, true)] // right occurrence; maxDay - date.Day == 6
	[TestCase(-3, 2004, 6, 17, false)] // wrong occurrence; maxDay - date.Day == -1
	[TestCase(-3, 2005, 6, 9, false)] // wrong occurrence; maxDay - date.Day == 7
	[TestCase(-3, 2005, 6, 16, true)] // right occurrence; maxDay - date.Day == 0
	public void Contains_WithValidDate_ReturnsExpected(
		Int32 occurrence, Int32 year, Int32 month, Int32 day, Boolean expectedResult)
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(6, DayOfWeek.Thursday, occurrence);
		Date date = new(year, month, day);

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	[Test]
	public void EnumerateBackwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(6, DayOfWeek.Thursday, 3);
		Date date = Date.Empty;

		// Act:
		Func<IEnumerable<Date>> func = () => schedule.EnumerateBackwardFrom(date);

		// Assert:
		func.Should().Throw<ArgumentException>().WithParameterName("date");
	}

	[TestCase(2000, 6, 15, 2000, new[] { 15, 17, 18 })]
	[TestCase(2000, 6, 14, 1999, new[] { 17, 18, 19 })]
	[TestCase(2, 12, 31, 2, new[] { 20, 21 })]
	public void EnumerateBackwardFrom_WithValidDate_ReturnsExpected(
		Int32 startYear, Int32 startMonth, Int32 startDay,
		Int32 expectedYear0, Int32[] expectedDays)
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(6, DayOfWeek.Thursday, 3);
		Date date = new(startYear, startMonth, startDay);

		// Act:
		Date[] actualResults = schedule.EnumerateBackwardFrom(date).Take(3).ToArray();

		// Assert:
		Date[] expectedResults = expectedDays
			.Select((day, index) => new Date(expectedYear0 - index, 6, day))
			.ToArray();
		actualResults.Should().Equal(expectedResults);
	}
	
	[Test]
	public void EnumerateForwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(6, DayOfWeek.Thursday, 3);
		Date date = Date.Empty;

		// Act:
		Func<IEnumerable<Date>> func = () => schedule.EnumerateForwardFrom(date);

		// Assert:
		func.Should().Throw<ArgumentException>().WithParameterName("date");
	}

	[TestCase(2000, 6, 15, 2000, new[] { 15, 21, 20 })]
	[TestCase(2000, 6, 16, 2001, new[] { 21, 20, 19 })]
	[TestCase(9998, 1, 1, 9998, new[] { 18, 17 })]
	public void EnumerateForwardFrom_WithValidDate_ReturnsExpected(
		Int32 startYear, Int32 startMonth, Int32 startDay,
		Int32 expectedYear0, Int32[] expectedDays)
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(6, DayOfWeek.Thursday, 3);
		Date date = new(startYear, startMonth, startDay);

		// Act:
		Date[] actualResults = schedule.EnumerateForwardFrom(date).Take(3).ToArray();

		// Assert:
		Date[] expectedResults = expectedDays
			.Select((day, index) => new Date(expectedYear0 + index, 6, day))
			.ToArray();
		actualResults.Should().Equal(expectedResults);
	}

	[TestCase(Date.MinYear - 1)]
	[TestCase(Date.MaxYear + 1)]
	public void GetOccurrence_WithInvalidYear_ThrowsException(Int32 year)
	{
		// Arrange:
		AnnualRecurrence recurrence = new AnnualDayOfWeekOfMonthRecurrence(6, DayOfWeek.Thursday, 3);

		// Act:
		Func<Date> func = () => recurrence.GetOccurrence(year);

		// Assert:
		func.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("year");
	}

	[TestCase(3, 1, 21)] // minimum year
	[TestCase(3, 2000, 15)] // minimum day
	[TestCase(3, 2001, 21)] // maximum day
	[TestCase(3, 2002, 20)]
	[TestCase(-3, 2003, 12)]
	[TestCase(-3, 2004, 10)] // minimum day
	[TestCase(-3, 2005, 16)] // maximum day
	[TestCase(-3, 9999, 10)] // maximum year
	public void GetOccurrence_WithValidYear_ReturnsExpected(Int32 occurrence, Int32 year, Int32 expectedDay)
	{
		// Arrange:
		const Int32 month = 6;
		AnnualRecurrence recurrence = new AnnualDayOfWeekOfMonthRecurrence(month, DayOfWeek.Thursday, occurrence);

		// Act:
		Date actualDate = recurrence.GetOccurrence(year);

		// Assert:
		actualDate.Year.Should().Be(year);
		actualDate.Month.Should().Be(month);
		actualDate.Day.Should().Be(expectedDay);
	}
}
