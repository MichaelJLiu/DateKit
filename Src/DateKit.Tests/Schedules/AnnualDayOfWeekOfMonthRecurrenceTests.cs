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
	#region Constructor

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
	public void Constructor_WithValidArgumentsAndDefaultOptions_ReturnsInstance(
		Int32 month, DayOfWeek dayOfWeek, Int32 occurrence)
	{
		// Act:
		AnnualDayOfWeekOfMonthRecurrence recurrence = new(month, dayOfWeek, occurrence);

		// Assert:
		recurrence.StartYear.Should().Be(Date.MinYear);
		recurrence.EndYear.Should().Be(Date.MaxYear);
		recurrence.Month.Should().Be(month);
		recurrence.DayOfWeek.Should().Be(dayOfWeek);
		recurrence.Occurrence.Should().Be(occurrence);
	}

	#endregion

	#region Contains

	[Test]
	public void Contains_WithEmptyDate_ReturnsFalse()
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(month: 6, DayOfWeek.Thursday, occurrence: 3);
		Date date = Date.Empty;

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().BeFalse();
	}

	[TestCaseSource(nameof(GetContainsTestCases))]
	public void Contains_WithValidDate_ReturnsExpected(
		Int32 month, DayOfWeek dayOfWeek, Int32 occurrence, Date date, Boolean expectedResult)
	{
		// Arrange:
		AnnualDayOfWeekOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(month, dayOfWeek, occurrence, options);

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	private static IEnumerable<Object[]> GetContainsTestCases()
	{
		static Object[] CreateTestCase(Int32 month, Int32 occurrence, Date date, Boolean expectedResult)
		{
			return [month, DayOfWeek.Thursday, occurrence, date, expectedResult];
		}

		static Object[] CreateFebruaryTestCase(DayOfWeek dayOfWeek, Int32 occurrence, Date date, Boolean expectedResult)
		{
			return [2, dayOfWeek, occurrence, date, expectedResult];
		}

		return
		[
			CreateTestCase(6, 3, new Date(1999, 6, 17), false), // before StartYear
			CreateTestCase(6, 3, new Date(2000, 4, 20), false), // wrong month
			CreateTestCase(6, 3, new Date(2000, 6, 14), false), // wrong day of week
			CreateTestCase(6, 3, new Date(2000, 6, 15), true), // right occurrence; maxDay - date.Day == 6
			CreateTestCase(6, 3, new Date(2000, 6, 22), false), // wrong occurrence; maxDay - date.Day == -1
			CreateTestCase(6, 3, new Date(2001, 6, 14), false), // wrong occurrence; maxDay - date.Day == 7
			CreateTestCase(6, 3, new Date(2001, 6, 21), true), // right occurrence; maxDay - date.Day == 0
			CreateTestCase(10, -3, new Date(2001, 10, 11), true), // right occurrence; maxDay - date.Day == 6
			CreateTestCase(10, -3, new Date(2001, 10, 18), false), // wrong occurrence; maxDay - date.Day == -1
			CreateTestCase(10, -3, new Date(2002, 10, 10), false), // wrong occurrence; maxDay - date.Day == 7
			CreateTestCase(10, -3, new Date(2002, 10, 17), true), // right occurrence; maxDay - date.Day == 0
			CreateTestCase(10, -3, new Date(2003, 10, 16), false), // after EndYear
			CreateFebruaryTestCase(DayOfWeek.Tuesday, 2, new Date(2000, 2, 8), true),
			CreateFebruaryTestCase(DayOfWeek.Tuesday, -2, new Date(2000, 2, 22), true), // leap year; maximum day
			CreateFebruaryTestCase(DayOfWeek.Wednesday, -2, new Date(2000, 2, 16), true), // leap year; minimum day
			CreateFebruaryTestCase(DayOfWeek.Wednesday, -2, new Date(2001, 2, 21), true), // common year; maximum day
			CreateFebruaryTestCase(DayOfWeek.Thursday, -2, new Date(2001, 2, 15), true), // common year; minimum day
		];
	}

	#endregion

	#region EnumerateBackwardFrom

	[Test]
	public void EnumerateBackwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(month: 6, DayOfWeek.Thursday, occurrence: 3);
		Date date = Date.Empty;

		// Act:
		Func<IEnumerable<Date>> func = () => schedule.EnumerateBackwardFrom(date);

		// Assert:
		func.Should().Throw<ArgumentException>().WithParameterName("date");
	}

	[TestCaseSource(nameof(GetEnumerateBackwardFromTestCases))]
	public void EnumerateBackwardFrom_WithValidDate_ReturnsExpected(Date date, Date[] expectedResults)
	{
		// Arrange:
		AnnualDayOfWeekOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(month: 6, DayOfWeek.Thursday, occurrence: 3, options);

		// Act:
		Date[] actualResults = schedule.EnumerateBackwardFrom(date).Take(3).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateBackwardFromTestCases()
	{
		static Object[] CreateTestCase(Date date, Int32 expectedYear0, Int32[] expectedDays)
		{
			Date[] expectedResults = expectedDays
				.Select((day, index) => new Date(expectedYear0 - index, 6, day))
				.ToArray();
			return [date, expectedResults];
		}

		return
		[
			CreateTestCase(new Date(1999, 12, 31), 0, []),
			CreateTestCase(new Date(2000, 6, 14), 0, []),
			CreateTestCase(new Date(2000, 6, 15), 2000, [15]),
			CreateTestCase(new Date(2002, 12, 31), 2002, [20, 21, 15]),
			CreateTestCase(new Date(2003, 12, 31), 2002, [20, 21, 15]),
		];
	}

	#endregion

	#region EnumerateForwardFrom

	[Test]
	public void EnumerateForwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(month: 6, DayOfWeek.Thursday, occurrence: 3);
		Date date = Date.Empty;

		// Act:
		Func<IEnumerable<Date>> func = () => schedule.EnumerateForwardFrom(date);

		// Assert:
		func.Should().Throw<ArgumentException>().WithParameterName("date");
	}

	[TestCaseSource(nameof(GetEnumerateForwardFromTestCases))]
	public void EnumerateForwardFrom_WithValidDate_ReturnsExpected(Date date, Date[] expectedResults)
	{
		// Arrange:
		AnnualDayOfWeekOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = new AnnualDayOfWeekOfMonthRecurrence(month: 6, DayOfWeek.Thursday, occurrence: 3, options);

		// Act:
		Date[] actualResults = schedule.EnumerateForwardFrom(date).Take(3).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateForwardFromTestCases()
	{
		static Object[] CreateTestCase(Date date, Int32 expectedYear0, Int32[] expectedDays)
		{
			Date[] expectedResults = expectedDays
				.Select((day, index) => new Date(expectedYear0 + index, 6, day))
				.ToArray();
			return [date, expectedResults];
		}

		return
		[
			CreateTestCase(new Date(1999, 1, 1), 2000, [15, 21, 20]),
			CreateTestCase(new Date(2000, 1, 1), 2000, [15, 21, 20]),
			CreateTestCase(new Date(2002, 6, 20), 2002, [20]),
			CreateTestCase(new Date(2002, 6, 21), 0, []),
			CreateTestCase(new Date(2003, 1, 1), 0, []),
		];
	}

	#endregion

	#region GetOccurrence

	[TestCase(Date.MinYear - 1)]
	[TestCase(1999)] // before StartYear
	[TestCase(2003)] // after EndYear
	[TestCase(Date.MaxYear + 1)]
	public void GetOccurrence_WithUnscheduledYear_ReturnsEmptyDate(Int32 year)
	{
		// Arrange:
		AnnualDayOfWeekOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		AnnualRecurrence recurrence =
			new AnnualDayOfWeekOfMonthRecurrence(month: 6, DayOfWeek.Thursday, occurrence: 3, options);

		// Act:
		Date actualResult = recurrence.GetOccurrence(year);

		// Assert:
		actualResult.Should().Be(Date.Empty);
	}

	[TestCase(DayOfWeek.Thursday, 3, 2000, 6, 15)] // StartYear; minimum day
	[TestCase(DayOfWeek.Thursday, 3, 2001, 6, 21)] // maximum day
	[TestCase(DayOfWeek.Thursday, 3, 2002, 6, 20)] // EndYear
	[TestCase(DayOfWeek.Thursday, -3, 2000, 10, 12)] // StartYear
	[TestCase(DayOfWeek.Thursday, -3, 2001, 10, 11)] // minimum day
	[TestCase(DayOfWeek.Thursday, -3, 2002, 10, 17)] // EndYear; maximum day
	// February
	[TestCase(DayOfWeek.Tuesday, 2, 2000, 2, 8)]
	[TestCase(DayOfWeek.Tuesday, -2, 2000, 2, 22)] // leap year; maximum day
	[TestCase(DayOfWeek.Wednesday, -2, 2000, 2, 16)] // leap year; minimum day
	[TestCase(DayOfWeek.Wednesday, -2, 2001, 2, 21)] // common year; maximum day
	[TestCase(DayOfWeek.Thursday, -2, 2001, 2, 15)] // common year; minimum day
	public void GetOccurrence_WithScheduledYear_ReturnsExpected(
		DayOfWeek dayOfWeek, Int32 occurrence, Int32 year, Int32 month, Int32 expectedDay)
	{
		// Arrange:
		AnnualDayOfWeekOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2005 };
		AnnualRecurrence recurrence =
			new AnnualDayOfWeekOfMonthRecurrence(month, dayOfWeek, occurrence, options);

		// Act:
		Date actualResult = recurrence.GetOccurrence(year);

		// Assert:
		actualResult.Year.Should().Be(year);
		actualResult.Month.Should().Be(month);
		actualResult.Day.Should().Be(expectedDay);
	}

	#endregion
}
