using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

using static DayOfWeek;

/// <summary>
/// Tests the <see cref="AnnualDayOfMonthRecurrence" /> class.
/// </summary>
[TestFixture]
public class AnnualDayOfMonthRecurrenceTests
{
	#region Create

	[TestCase(0, 1, "month")]
	[TestCase(1, 0, "day")]
	[TestCase(2, 29, "day")]
	[TestCase(12, 32, "day")]
	[TestCase(13, 31, "month")]
	public void Create_WithArgumentOutOfRange_ThrowsException(Int32 month, Int32 day, String expectedParamName)
	{
		// Act:
		Func<AnnualDayOfMonthRecurrence> func = () => AnnualDayOfMonthRecurrence.Create(month, day);

		// Assert:
		func.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(expectedParamName);
	}

	[Test]
	public void Create_WithNullOptions_ThrowsException()
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options = null!;

		// Act:
		Func<AnnualDayOfMonthRecurrence> func = () => AnnualDayOfMonthRecurrence.Create(month: 6, day: 15, options);

		// Assert:
		func.Should().Throw<ArgumentNullException>().WithParameterName("options");
	}

	[TestCase(1, 1)]
	[TestCase(2, 28)]
	[TestCase(12, 31)]
	public void Create_WithValidArgumentsAndDefaultOptions_ReturnsInstance(Int32 month, Int32 day)
	{
		// Act:
		AnnualDayOfMonthRecurrence recurrence = AnnualDayOfMonthRecurrence.Create(month, day);

		// Assert:
		recurrence.StartYear.Should().Be(Date.MinYear);
		recurrence.EndYear.Should().Be(Date.MaxYear);
		recurrence.Month.Should().Be(month);
		recurrence.Day.Should().Be(day);

		DayOfWeekAdjustments dayOfWeekAdjustments = recurrence.DayOfWeekAdjustments;
		for (DayOfWeek dayOfWeek = Sunday; dayOfWeek <= Saturday; ++dayOfWeek)
			dayOfWeekAdjustments[dayOfWeek].Should().Be(0);
	}

	#endregion

	#region DayOfWeekAdjustments

	[Test]
	public void DayOfWeekAdjustments_ReturnsSnapshot()
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options =
			new()
			{
				DayOfWeekAdjustments = new DayOfWeekAdjustments { [Sunday] = 1 },
			};
		AnnualDayOfMonthRecurrence recurrence = AnnualDayOfMonthRecurrence.Create(month: 6, day: 15, options);

		// Act:
		DayOfWeekAdjustments adjustments = recurrence.DayOfWeekAdjustments;
		adjustments[Sunday] = -1;

		// Assert:
		adjustments[Sunday].Should().Be(-1);
		recurrence.DayOfWeekAdjustments[Sunday].Should().Be(1);
	}

	#endregion

	// 1998 Calendar        | 1999 Calendar
	// June                 | June
	// Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa
	// 14‹15›16 17 18 19 20 | 13 14‹15›16 17 18 19
	//
	// 2000 Calendar
	// January              | June                 | December
	// Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa
	//                   ‹1›|  4  5  6  7  8  9 10 |
	//  2  3  4  5  6  7  8 | 11 12 13 14‹15›16 17 | 24 25 26 27 28 29 30
	//                      | 18 19 20 21 22 23 24 |‹31›
	//
	// 2001 Calendar        | 2002 Calendar
	// June                 | June
	// Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa
	// 10 11 12 13 14‹15›16 |  9 10 11 12 13 14‹15›

	#region Contains

	[TestCaseSource(nameof(GetContainsTestCases))]
	public void Contains_WithValidDate_ReturnsExpected(
		Int32 startYear, Int32 endYear, Int32 month, Int32 day, Date date, Boolean expectedResult)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options =
			new()
			{
				StartYear = startYear,
				EndYear = endYear,
				DayOfWeekAdjustments =
					new DayOfWeekAdjustments
					{
						[Sunday] = 2,
						[Monday] = 1,
						[Friday] = -1,
						[Saturday] = -2,
					},
			};
		Schedule schedule = AnnualDayOfMonthRecurrence.Create(month, day, options);

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	private static IEnumerable<Object[]> GetContainsTestCases()
	{
		return
		[
			// Saturday, January 1, 2000 -> Thursday, December 30, 1999
			CreateTestCase(0001, 1999, 1, 1, new Date(1999, 12, 30), false), // after EndYear
			CreateTestCase(0001, 2000, 1, 1, new Date(1999, 12, 30), true),
			CreateTestCase(0001, 9999, 1, 1, new Date(1999, 12, 31), false), // wrong day
			CreateTestCase(2000, 9999, 1, 1, new Date(1999, 12, 30), true),
			CreateTestCase(2001, 9999, 1, 1, new Date(1999, 12, 30), false), // before StartYear
			// Thursday, June 15, 2000
			CreateTestCase(0001, 1999, 6, 15, new Date(2000, 6, 15), false), // after EndYear
			CreateTestCase(0001, 2000, 6, 15, new Date(2000, 6, 15), true),
			CreateTestCase(0001, 9999, 6, 15, new Date(2000, 6, 14), false), // wrong day
			CreateTestCase(2000, 9999, 6, 15, new Date(2000, 6, 15), true),
			CreateTestCase(2001, 9999, 6, 15, new Date(2000, 6, 15), false), // before StartYear
			// Friday, June 15, 2001 -> Thursday, June 14, 2001
			CreateTestCase(0001, 9999, 6, 15, new Date(2001, 6, 14), true),
			CreateTestCase(0001, 9999, 6, 15, new Date(2001, 6, 15), false), // wrong day
			// Sunday, December 31, 2000 -> Tuesday, January 2, 2001
			CreateTestCase(0001, 1999, 12, 31, new Date(2001, 1, 2), false), // after EndYear
			CreateTestCase(0001, 2000, 12, 31, new Date(2001, 1, 2), true),
			CreateTestCase(0001, 9999, 12, 31, new Date(2001, 1, 1), false), // wrong day
			CreateTestCase(2000, 9999, 12, 31, new Date(2001, 1, 2), true),
			CreateTestCase(2001, 9999, 12, 31, new Date(2001, 1, 2), false), // before StartYear
		];

		static Object[] CreateTestCase(
			Int32 startYear, Int32 endYear, Int32 month, Int32 day, Date date, Boolean expectedResult)
		{
			return [startYear, endYear, month, day, date, expectedResult];
		}
	}

	#endregion

	#region EnumerateBackwardFrom

	[TestCaseSource(nameof(GetEnumerateBackwardFromTestCases))]
	public void EnumerateBackwardFrom_WithValidDate_ReturnsExpected(
		Int32 month, Int32 day, Date date, Date[] expectedResults)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options =
			new()
			{
				StartYear = 2000,
				EndYear = 2002,
				DayOfWeekAdjustments =
					new DayOfWeekAdjustments
					{
						[Sunday] = 2,
						[Monday] = 1,
						[Friday] = -1,
						[Saturday] = -2,
					},
			};
		Schedule schedule = AnnualDayOfMonthRecurrence.Create(month, day, options);

		// Act:
		Date[] actualResults = schedule.EnumerateBackwardFrom(date).Take(4).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateBackwardFromTestCases()
	{
		return
		[
			CreateTestCase(1, 1, new Date(1999, 12, 30), 2000, [-2]), // start from next year
			CreateTestCase(6, 15, new Date(1999, 12, 31), 0, []), // start before StartYear
			CreateTestCase(6, 15, new Date(2000, 6, 14), 0, []),
			CreateTestCase(6, 15, new Date(2000, 6, 15), 2000, [0]),
			CreateTestCase(6, 15, new Date(2002, 12, 31), 2002, [-2, -1, 0]),
			CreateTestCase(6, 15, new Date(2003, 12, 31), 2002, [-2, -1, 0]), // start after EndYear
		];

		static Object[] CreateTestCase(
			Int32 month, Int32 day, Date date, Int32 expectedYear0, Int32[] expectedDayOffsets)
		{
			Date[] expectedResults = expectedDayOffsets
				.Select((offset, index) => new Date(expectedYear0 - index, month, day).AddDays(offset))
				.ToArray();
			return [month, day, date, expectedResults];
		}
	}

	#endregion

	#region EnumerateForwardFrom

	[TestCaseSource(nameof(GetEnumerateForwardFromTestCases))]
	public void EnumerateForwardFrom_WithValidDate_ReturnsExpected(
		Int32 month, Int32 day, Date date, Date[] expectedResults)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options =
			new()
			{
				StartYear = 1998,
				EndYear = 2000,
				DayOfWeekAdjustments =
					new DayOfWeekAdjustments
					{
						[Sunday] = 2,
						[Monday] = 1,
						[Friday] = -1,
						[Saturday] = -2,
					},
			};
		Schedule schedule = AnnualDayOfMonthRecurrence.Create(month, day, options);

		// Act:
		Date[] actualResults = schedule.EnumerateForwardFrom(date).Take(4).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateForwardFromTestCases()
	{
		return
		[
			CreateTestCase(6, 15, new Date(1997, 1, 1), 1998, [1, 0, 0]), // start before StartYear
			CreateTestCase(6, 15, new Date(1998, 1, 1), 1998, [1, 0, 0]),
			CreateTestCase(6, 15, new Date(2000, 6, 15), 2000, [0]),
			CreateTestCase(6, 15, new Date(2000, 6, 16), 0, []),
			CreateTestCase(6, 15, new Date(2001, 1, 1), 0, []), // start after EndYear
			CreateTestCase(12, 31, new Date(2001, 1, 2), 2000, [2]), // start from previous year
		];

		static Object[] CreateTestCase(
			Int32 month, Int32 day, Date date, Int32 expectedYear0, Int32[] expectedDayOffsets)
		{
			Date[] expectedResults = expectedDayOffsets
				.Select((offset, index) => new Date(expectedYear0 + index, month, day).AddDays(offset))
				.ToArray();
			return [month, day, date, expectedResults];
		}
	}

	#endregion

	#region GetOccurrence

	[TestCase(1, 1, -2)] // Saturday -> Thursday
	[TestCase(6, 15, 0)]
	[TestCase(12, 31, 2)] // Sunday -> Tuesday
	public void GetOccurrence_WithScheduledYear_ReturnsExpected(Int32 month, Int32 day, Int32 expectedDayOffset)
	{
		// Arrange:
		const Int32 year = 2000;
		AnnualDayOfMonthRecurrenceOptions options =
			new()
			{
				DayOfWeekAdjustments =
					new DayOfWeekAdjustments
					{
						[Sunday] = 2,
						[Monday] = 1,
						[Friday] = -1,
						[Saturday] = -2,
					},
			};
		AnnualDayOfMonthRecurrence recurrence = AnnualDayOfMonthRecurrence.Create(month, day, options);

		// Act:
		Date actualResult = recurrence.GetOccurrence(year);

		// Assert:
		actualResult.Should().Be(new Date(year, month, day).AddDays(expectedDayOffset));
	}

	#endregion
}
