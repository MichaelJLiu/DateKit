using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

using static DayOfWeek;

/// <summary>
/// Tests the <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> class.
/// </summary>
[TestFixture]
public class AnnualNthDayOfWeekOfMonthRecurrenceTests
{
	#region Constructor

	[TestCase(0, Sunday, -4, "month")]
	[TestCase(1, -1, -1, "dayOfWeek")]
	[TestCase(3, Monday, -5, "instance")]
	[TestCase(6, Wednesday, 0, "instance")]
	[TestCase(9, Thursday, 5, "instance")]
	[TestCase(12, 7, 1, "dayOfWeek")]
	[TestCase(13, Saturday, 4, "month")]
	public void Constructor_WithArgumentOutOfRange_ThrowsException(
		Int32 month, DayOfWeek dayOfWeek, Int32 instance, String expectedParamName)
	{
		// Act:
		Func<AnnualNthDayOfWeekOfMonthRecurrence> func = () =>
			new AnnualNthDayOfWeekOfMonthRecurrence(month, dayOfWeek, instance);

		// Assert:
		func.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(expectedParamName);
	}

	[Test]
	public void Constructor_WithNullOptions_ThrowsException()
	{
		// Arrange:
		AnnualNthDayOfWeekOfMonthRecurrenceOptions options = null!;

		// Act:
		Func<AnnualNthDayOfWeekOfMonthRecurrence> func = () =>
			new AnnualNthDayOfWeekOfMonthRecurrence(month: 6, Thursday, instance: 3, options);

		// Assert:
		func.Should().Throw<ArgumentNullException>().WithParameterName("options");
	}

	[TestCase(1, Sunday, -4)]
	[TestCase(2, Monday, -1)]
	[TestCase(11, Friday, 1)]
	[TestCase(12, Saturday, 4)]
	public void Constructor_WithValidArgumentsAndDefaultOptions_ReturnsInstance(
		Int32 month, DayOfWeek dayOfWeek, Int32 instance)
	{
		// Act:
		AnnualNthDayOfWeekOfMonthRecurrence recurrence = new(month, dayOfWeek, instance);

		// Assert:
		recurrence.StartYear.Should().Be(Date.MinYear);
		recurrence.EndYear.Should().Be(Date.MaxYear);
		recurrence.Month.Should().Be(month);
		recurrence.DayOfWeek.Should().Be(dayOfWeek);
		recurrence.Instance.Should().Be(instance);
	}

	#endregion

	// 2000 Calendar
	// February             | June                 | October
	// Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa
	//        1  2  3  4  5 |              1  2  3 |  1  2  3  4  5  6  7
	//  6  7 ‹8› 9 10 11 12 |  4  5  6  7  8  9 10 |  8  9 10 11‹12›13 14
	// 13 14 15‹16›17 18 19 | 11 12 13 14‹15›16 17 | 15 16 17 18 19 20 21
	// 20 21‹22›23 24 25 26 | 18 19 20 21 22 23 24 | 22 23 24 25 26 27 28
	// 27 28 29             | 25 26 27 28 29 30    | 29 30 31
	//                        3rd Thursday falls
	//                        on minimum day
	// 2001 Calendar
	// February             | June                 | October
	// Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa
	//              1  2  3 |                 1  2 |     1  2  3  4  5  6
	//  4  5  6  7  8  9 10 |  3  4  5  6  7  8  9 |  7  8  9 10‹11›12 13
	// 11 12 13 14‹15›16 17 | 10 11 12 13 14 15 16 | 14 15 16 17 18 19 20
	// 18 19 20‹21›22 23 24 | 17 18 19 20‹21›22 23 | 21 22 23 24 25 26 27
	// 25 26 27 28          | 24 25 26 27 28 29 30 | 28 29 30 31
	//                        3rd Thursday falls     3rd-to-last Thursday
	//                        on maximum day         falls on minimum day
	// 2002 Calendar
	// June                 | October
	// Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa
	//                    1 |        1  2  3  4  5
	//  2  3  4  5  6  7  8 |  6  7  8  9 10 11 12
	//  9 10 11 12 13 14 15 | 13 14 15 16‹17›18 19
	// 16 17 18 19‹20›21 22 | 20 21 22 23 24 25 26
	// 23 24 25 26 27 28 29 | 27 28 29 30 31
	// 30                     3rd-to-last Thursday
	//                        falls on maximum day

	#region Contains

	[TestCaseSource(nameof(GetContainsTestCases))]
	public void Contains_WithValidDate_ReturnsExpected(
		Int32 month, DayOfWeek dayOfWeek, Int32 instance, Date date, Boolean expectedResult)
	{
		// Arrange:
		AnnualNthDayOfWeekOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = new AnnualNthDayOfWeekOfMonthRecurrence(month, dayOfWeek, instance, options);

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	private static IEnumerable<Object[]> GetContainsTestCases()
	{
		return
		[
			CreateTestCase(6, 3, new Date(1999, 6, 17), false), // before StartYear
			CreateTestCase(6, 3, new Date(2000, 2, 17), false), // wrong month
			CreateTestCase(6, 3, new Date(2000, 6, 14), false), // wrong day of week
			CreateTestCase(6, 3, new Date(2000, 6, 15), true), // right instance; maxDay - day == 6
			CreateTestCase(6, 3, new Date(2000, 6, 22), false), // wrong instance; maxDay - day == -1
			CreateTestCase(6, 3, new Date(2001, 6, 14), false), // wrong instance; maxDay - day == 7
			CreateTestCase(6, 3, new Date(2001, 6, 21), true), // right instance; maxDay - day == 0
			CreateTestCase(10, -3, new Date(2001, 10, 11), true), // right instance; maxDay - day == 6
			CreateTestCase(10, -3, new Date(2001, 10, 18), false), // wrong instance; maxDay - day == -1
			CreateTestCase(10, -3, new Date(2002, 10, 10), false), // wrong instance; maxDay - day == 7
			CreateTestCase(10, -3, new Date(2002, 10, 17), true), // right instance; maxDay - day == 0
			CreateTestCase(10, -3, new Date(2003, 10, 16), false), // after EndYear
			CreateFebruaryTestCase(Tuesday, 2, new Date(2000, 2, 8), true),
			CreateFebruaryTestCase(Tuesday, -2, new Date(2000, 2, 22), true), // leap year; maximum day
			CreateFebruaryTestCase(Wednesday, -2, new Date(2000, 2, 16), true), // leap year; minimum day
			CreateFebruaryTestCase(Wednesday, -2, new Date(2001, 2, 21), true), // common year; maximum day
			CreateFebruaryTestCase(Thursday, -2, new Date(2001, 2, 15), true), // common year; minimum day
		];

		static Object[] CreateTestCase(Int32 month, Int32 instance, Date date, Boolean expectedResult)
		{
			return [month, Thursday, instance, date, expectedResult];
		}

		static Object[] CreateFebruaryTestCase(DayOfWeek dayOfWeek, Int32 instance, Date date, Boolean expectedResult)
		{
			return [2, dayOfWeek, instance, date, expectedResult];
		}
	}

	#endregion

	#region EnumerateBackwardFrom

	[TestCaseSource(nameof(GetEnumerateBackwardFromTestCases))]
	public void EnumerateBackwardFrom_WithValidDate_ReturnsExpected(Date date, Date[] expectedResults)
	{
		// Arrange:
		AnnualNthDayOfWeekOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = new AnnualNthDayOfWeekOfMonthRecurrence(month: 6, Thursday, instance: 3, options);

		// Act:
		Date[] actualResults = schedule.EnumerateBackwardFrom(date).Take(3).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateBackwardFromTestCases()
	{
		return
		[
			CreateTestCase(new Date(1999, 12, 31), 0, []), // start before StartYear
			CreateTestCase(new Date(2000, 6, 14), 0, []),
			CreateTestCase(new Date(2000, 6, 15), 2000, [15]),
			CreateTestCase(new Date(2002, 12, 31), 2002, [20, 21, 15]),
			CreateTestCase(new Date(2003, 12, 31), 2002, [20, 21, 15]), // start after EndYear
		];

		static Object[] CreateTestCase(Date date, Int32 expectedYear0, Int32[] expectedDays)
		{
			Date[] expectedResults = expectedDays
				.Select((day, index) => new Date(expectedYear0 - index, 6, day))
				.ToArray();
			return [date, expectedResults];
		}
	}

	#endregion

	#region EnumerateForwardFrom

	[TestCaseSource(nameof(GetEnumerateForwardFromTestCases))]
	public void EnumerateForwardFrom_WithValidDate_ReturnsExpected(Date date, Date[] expectedResults)
	{
		// Arrange:
		AnnualNthDayOfWeekOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = new AnnualNthDayOfWeekOfMonthRecurrence(month: 6, Thursday, instance: 3, options);

		// Act:
		Date[] actualResults = schedule.EnumerateForwardFrom(date).Take(3).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateForwardFromTestCases()
	{
		return
		[
			CreateTestCase(new Date(1999, 1, 1), 2000, [15, 21, 20]), // start before StartYear
			CreateTestCase(new Date(2000, 1, 1), 2000, [15, 21, 20]),
			CreateTestCase(new Date(2002, 6, 20), 2002, [20]),
			CreateTestCase(new Date(2002, 6, 21), 0, []),
			CreateTestCase(new Date(2003, 1, 1), 0, []), // start after EndYear
		];

		static Object[] CreateTestCase(Date date, Int32 expectedYear0, Int32[] expectedDays)
		{
			Date[] expectedResults = expectedDays
				.Select((day, index) => new Date(expectedYear0 + index, 6, day))
				.ToArray();
			return [date, expectedResults];
		}
	}

	#endregion

	#region GetDayOfYearRange

	[TestCase(1, Monday, 1)] // 2001-01-01, 2002-01-07
	[TestCase(2, Tuesday, -2)] // 2005-02-22, 2000-02-29
	public void GetDayOfYearRange_ReturnsExpected(Int32 month, DayOfWeek dayOfWeek, Int32 instance)
	{
		// Arrange:
		AnnualRecurrence recurrence = new AnnualNthDayOfWeekOfMonthRecurrence(month, dayOfWeek, instance);

		// Act:
		recurrence.GetDayOfYearRange(out Int32 actualMinDayOfYear, out Int32 actualMaxDayOfYear);

		// Assert:
		Int32[] daysOfYear = Enumerable.Range(2000, 6)
			.Select(year => recurrence.GetOccurrence(year).DayOfYear)
			.ToArray();
		actualMinDayOfYear.Should().Be(daysOfYear.Min());
		actualMaxDayOfYear.Should().Be(daysOfYear.Max());
	}

	#endregion

	#region GetOccurrence

	[TestCase(Thursday, 3, 2000, 6, 15)] // StartYear; minimum day
	[TestCase(Thursday, 3, 2001, 6, 21)] // maximum day
	[TestCase(Thursday, 3, 2002, 6, 20)] // EndYear
	[TestCase(Thursday, -3, 2000, 10, 12)] // StartYear
	[TestCase(Thursday, -3, 2001, 10, 11)] // minimum day
	[TestCase(Thursday, -3, 2002, 10, 17)] // EndYear; maximum day
	// February
	[TestCase(Tuesday, 2, 2000, 2, 8)]
	[TestCase(Tuesday, -2, 2000, 2, 22)] // leap year; maximum day
	[TestCase(Wednesday, -2, 2000, 2, 16)] // leap year; minimum day
	[TestCase(Wednesday, -2, 2001, 2, 21)] // common year; maximum day
	[TestCase(Thursday, -2, 2001, 2, 15)] // common year; minimum day
	public void GetOccurrence_WithScheduledYear_ReturnsExpected(
		DayOfWeek dayOfWeek, Int32 instance, Int32 year, Int32 month, Int32 expectedDay)
	{
		// Arrange:
		AnnualNthDayOfWeekOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		AnnualRecurrence recurrence =
			new AnnualNthDayOfWeekOfMonthRecurrence(month, dayOfWeek, instance, options);

		// Act:
		Date actualResult = recurrence.GetOccurrence(year);

		// Assert:
		actualResult.Year.Should().Be(year);
		actualResult.Month.Should().Be(month);
		actualResult.Day.Should().Be(expectedDay);
	}

	#endregion
}
