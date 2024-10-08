using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="AnnualDayOfMonthRecurrence" /> class.
/// </summary>
[TestFixture]
public class AnnualDayOfMonthRecurrenceTests
{
	#region Constructor

	[TestCase(0, 1, "month")]
	[TestCase(1, 0, "day")]
	[TestCase(2, 29, "day")]
	[TestCase(12, 32, "day")]
	[TestCase(13, 31, "month")]
	public void Constructor_WithInvalidArguments_ThrowsException(Int32 month, Int32 day, String expectedParamName)
	{
		// Act:
		Func<AnnualDayOfMonthRecurrence> func = () => new AnnualDayOfMonthRecurrence(month, day);

		// Assert:
		func.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(expectedParamName);
	}

	[TestCase(1, 1)]
	[TestCase(2, 28)]
	[TestCase(12, 31)]
	public void Constructor_WithValidArgumentsAndDefaultOptions_ReturnsInstance(Int32 month, Int32 day)
	{
		// Act:
		AnnualDayOfMonthRecurrence recurrence = new(month, day);

		// Assert:
		recurrence.StartYear.Should().Be(Date.MinYear);
		recurrence.EndYear.Should().Be(Date.MaxYear);
		recurrence.Month.Should().Be(month);
		recurrence.Day.Should().Be(day);
		DayOfWeekAdjustments adjustments = recurrence.DayOfWeekAdjustments;
		foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
			adjustments[dayOfWeek].Should().Be(0);
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
				DayOfWeekAdjustments = new DayOfWeekAdjustments { [DayOfWeek.Saturday] = -1 },
			};
		AnnualDayOfMonthRecurrence recurrence = new(month: 6, day: 15, options);
		DayOfWeekAdjustments adjustments = recurrence.DayOfWeekAdjustments;

		// Act:
		adjustments[DayOfWeek.Saturday] = 0;

		// Assert:
		adjustments[DayOfWeek.Saturday].Should().Be(0);
		recurrence.DayOfWeekAdjustments[DayOfWeek.Saturday].Should().Be(-1);
	}

	#endregion

	#region Contains

	[Test]
	public void Contains_WithEmptyDate_ReturnsFalse()
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfMonthRecurrence(month: 6, day: 15);
		Date date = Date.Empty;

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().BeFalse();
	}

	[TestCase(1999, 6, 15, false)] // Tuesday; before StartYear
	[TestCase(2000, 5, 15, false)] // Monday; wrong month
	[TestCase(2000, 6, 14, false)] // Wednesday; wrong day
	[TestCase(2000, 6, 15, true)] // Thursday
	[TestCase(2002, 6, 14, true)] // Friday (adjusted from Saturday)
	[TestCase(2002, 6, 15, false)] // Saturday
	[TestCase(2003, 6, 15, false)] // Sunday; after EndYear
	public void Contains_WithValidDate_ReturnsExpected(Int32 year, Int32 month, Int32 day, Boolean expectedResult)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options =
			new()
			{
				StartYear = 2000,
				EndYear = 2002,
				DayOfWeekAdjustments = new DayOfWeekAdjustments { [DayOfWeek.Saturday] = -1 },
			};
		Schedule schedule = new AnnualDayOfMonthRecurrence(month: 6, day: 15, options);
		Date date = new(year, month, day);

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	[TestCase(1999, 12, 31, -1, true)] // Friday (adjusted from Saturday)
	[TestCase(1999, 12, 31, 0, false)] // Friday
	[TestCase(9999, 12, 31, -1, false)] // Friday
	public void Contains_WithValidDateAdjustedToPreviousYear_ReturnsExpected(
		Int32 year, Int32 month, Int32 day, Int32 saturdayAdjustment, Boolean expectedResult)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options =
			new()
			{
				DayOfWeekAdjustments = new DayOfWeekAdjustments { [DayOfWeek.Saturday] = saturdayAdjustment },
			};
		Schedule schedule = new AnnualDayOfMonthRecurrence(month: 1, day: 1, options);
		Date date = new(year, month, day);

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	[TestCase(1, 1, 1, 1, false)] // Monday
	[TestCase(2001, 1, 1, 0, false)] // Monday
	[TestCase(2001, 1, 1, 1, true)] // Monday (adjusted from Sunday)
	public void Contains_WithValidDateAdjustedToNextYear_ReturnsExpected(
		Int32 year, Int32 month, Int32 day, Int32 sundayAdjustment, Boolean expectedResult)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options =
			new()
			{
				DayOfWeekAdjustments = new DayOfWeekAdjustments { [DayOfWeek.Sunday] = sundayAdjustment },
			};
		Schedule schedule = new AnnualDayOfMonthRecurrence(month: 12, day: 31, options);
		Date date = new(year, month, day);

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	#endregion

	#region EnumerateBackwardFrom

	[Test]
	public void EnumerateBackwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfMonthRecurrence(month: 6, day: 15);
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
		AnnualDayOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = new AnnualDayOfMonthRecurrence(month: 6, day: 15, options);

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
			CreateTestCase(new Date(2002, 12, 31), 2002, [15, 15, 15]),
			CreateTestCase(new Date(2003, 12, 31), 2002, [15, 15, 15]),
		];
	}

	#endregion

	#region EnumerateForwardFrom

	[Test]
	public void EnumerateForwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new AnnualDayOfMonthRecurrence(month: 6, day: 15);
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
		AnnualDayOfMonthRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = new AnnualDayOfMonthRecurrence(month: 6, day: 15, options);

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
			CreateTestCase(new Date(1999, 1, 1), 2000, [15, 15, 15]),
			CreateTestCase(new Date(2000, 1, 1), 2000, [15, 15, 15]),
			CreateTestCase(new Date(2002, 6, 15), 2002, [15]),
			CreateTestCase(new Date(2002, 6, 16), 0, []),
			CreateTestCase(new Date(2003, 1, 1), 0, []),
		];
	}

	#endregion

	#region GetOccurrence

	[TestCase(Date.MinYear - 1)]
	[TestCase(1997)] // before StartYear
	[TestCase(2003)] // after EndYear
	[TestCase(Date.MaxYear + 1)]
	public void GetOccurrence_WithUnscheduledYear_ReturnsEmptyDate(Int32 year)
	{
		// Arrange:
		AnnualDayOfMonthRecurrenceOptions options = new() { StartYear = 1998, EndYear = 2002 };
		AnnualRecurrence recurrence = new AnnualDayOfMonthRecurrence(month: 6, day: 15, options);

		// Act:
		Date actualResult = recurrence.GetOccurrence(year);

		// Assert:
		actualResult.Should().Be(Date.Empty);
	}

	[TestCase(1998, 18)] // StartYear
	[TestCase(2000, 15)]
	[TestCase(2002, 13)] // EndYear
	public void GetOccurrence_WithScheduledYear_ReturnsExpected(Int32 year, Int32 expectedDay)
	{
		// Arrange:
		const Int32 month = 6;
		AnnualDayOfMonthRecurrenceOptions options =
			new()
			{
				StartYear = 1998,
				EndYear = 2002,
				DayOfWeekAdjustments =
					new DayOfWeekAdjustments
					{
						[DayOfWeek.Sunday] = 4,
						[DayOfWeek.Monday] = 3,
						[DayOfWeek.Tuesday] = 2,
						[DayOfWeek.Wednesday] = 1,
						[DayOfWeek.Thursday] = 0,
						[DayOfWeek.Friday] = -1,
						[DayOfWeek.Saturday] = -2,
					},
			};
		AnnualRecurrence recurrence = new AnnualDayOfMonthRecurrence(month, day: 15, options);

		// Act:
		Date actualResult = recurrence.GetOccurrence(year);

		// Assert:
		actualResult.Year.Should().Be(year);
		actualResult.Month.Should().Be(month);
		actualResult.Day.Should().Be(expectedDay);
	}

	#endregion
}
