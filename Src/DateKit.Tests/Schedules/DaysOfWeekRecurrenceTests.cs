using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

using static DaysOfWeek;

/// <summary>
/// Tests the <see cref="DaysOfWeekRecurrence" /> class.
/// </summary>
[TestFixture]
public class DaysOfWeekRecurrenceTests
{
	#region Create

	[TestCase(0, "The value must specify one or more days of the week.*")]
	[TestCase(-1, "The value specifies an invalid day of the week.*")]
	public void Create_WithArgumentOutOfRange_ThrowsException(DaysOfWeek daysOfWeek, String expectedMessage)
	{
		// Act:
		Func<DaysOfWeekRecurrence> func = () => DaysOfWeekRecurrence.Create(daysOfWeek);

		// Assert:
		func.Should().Throw<ArgumentOutOfRangeException>()
			.WithParameterName("daysOfWeek")
			.WithMessage(expectedMessage);
	}

	[Test]
	public void Create_WithNullOptions_ThrowsException()
	{
		// Arrange:
		DaysOfWeekRecurrenceOptions options = null!;

		// Act:
		Func<DaysOfWeekRecurrence> func = () => DaysOfWeekRecurrence.Create(Sunday | Saturday, options);

		// Assert:
		func.Should().Throw<ArgumentNullException>().WithParameterName("options");
	}

	[Test]
	public void Create_WithValidArgumentAndDefaultOptions_ReturnsInstance()
	{
		// Act:
		DaysOfWeekRecurrence recurrence = DaysOfWeekRecurrence.Create(Sunday | Saturday);

		// Assert:
		recurrence.StartDate.Should().Be(new Date(1, 1, 6)); // Saturday
		recurrence.EndDate.Should().Be(new Date(9999, 12, 26)); // Sunday
		recurrence.DaysOfWeek.Should().Be(Sunday | Saturday);
	}

	#endregion

	// 2000 Calendar
	// January              | June                 | December
	// Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa | Su Mo Tu We Th Fr Sa
	//                   ‹1›|  4  5  6  7  8  9 10 |
	//  2  3  4  5  6  7  8 | 11 12 13 14‹15›16 17 | 24 25 26 27 28 29 30
	//                      | 18 19 20 21 22 23 24 |‹31›

	#region Contains

	[TestCaseSource(nameof(GetContainsTestCases))]
	public void Contains_WithValidDate_ReturnsExpected(DaysOfWeek daysOfWeek, Date date, Boolean expectedResult)
	{
		// Arrange:
		DaysOfWeekRecurrenceOptions options =
			new() { StartDate = new Date(2000, 1, 1), EndDate = new Date(2000, 12, 31) };
		Schedule schedule = DaysOfWeekRecurrence.Create(daysOfWeek, options);

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	private static IEnumerable<Object[]> GetContainsTestCases()
	{
		return
		[
			CreateTestCase(new Date(1999, 12, 31), Friday | Saturday, false), // before StartDate
			CreateTestCase(new Date(2000, 1, 1), Friday | Saturday, true), // StartDate
			CreateTestCase(new Date(2000, 6, 15), Wednesday | Friday, false),
			CreateTestCase(new Date(2000, 6, 15), Thursday, true),
			CreateTestCase(new Date(2000, 6, 15), Wednesday | Thursday | Friday, true),
			CreateTestCase(new Date(2000, 12, 31), Sunday | Monday, true), // EndDate
			CreateTestCase(new Date(2001, 1, 1), Sunday | Monday, false), // after EndDate
		];
	}

	private static Object[] CreateTestCase(Date date, DaysOfWeek daysOfWeek, Boolean expectedResult)
	{
		return [daysOfWeek, date, expectedResult];
	}

	#endregion

	#region EnumerateBackwardFrom

	[TestCaseSource(nameof(GetEnumerateBackwardFromTestCases))]
	public void EnumerateBackwardFrom_WithValidDate_ReturnsExpected(
		DaysOfWeek daysOfWeek, Date date, Date[] expectedResults)
	{
		// Arrange:
		DaysOfWeekRecurrenceOptions options =
			new() { StartDate = new Date(2000, 1, 1), EndDate = new Date(2000, 12, 31) };
		Schedule schedule = DaysOfWeekRecurrence.Create(daysOfWeek, options);

		// Act:
		Date[] actualResults = schedule.EnumerateBackwardFrom(date).Take(3).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateBackwardFromTestCases()
	{
		return
		[
			CreateTestCase(new Date(2000, 1, 6), Friday, []), // start before StartDate
			CreateTestCase(new Date(2000, 1, 7), Friday, [0]), // start and end on StartDate
			CreateTestCase(new Date(2000, 1, 7), Saturday, [-6]), // start on unscheduled date; end on StartDate
			CreateTestCase(new Date(2000, 1, 8), Saturday, [0, -7]), // start on scheduled date; end on StartDate
			CreateTestCase(new Date(2000, 6, 15), Tuesday | Thursday, [0, -2, -7]), // start on scheduled date
			CreateTestCase(new Date(2000, 6, 15), Sunday | Saturday, [-4, -5, -11]), // start on unscheduled date
			CreateTestCase(new Date(2001, 1, 1), Sunday | Monday, [-1, -7, -8]), // start after EndDate
		];
	}

	#endregion

	#region EnumerateForwardFrom

	[TestCaseSource(nameof(GetEnumerateForwardFromTestCases))]
	public void EnumerateForwardFrom_WithValidDate_ReturnsExpected(
		DaysOfWeek daysOfWeek, Date date, Date[] expectedResults)
	{
		// Arrange:
		DaysOfWeekRecurrenceOptions options =
			new() { StartDate = new Date(2000, 1, 1), EndDate = new Date(2000, 12, 31) };
		Schedule schedule = DaysOfWeekRecurrence.Create(daysOfWeek, options);

		// Act:
		Date[] actualResults = schedule.EnumerateForwardFrom(date).Take(3).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateForwardFromTestCases()
	{
		return
		[
			CreateTestCase(new Date(1999, 12, 31), Friday | Saturday, [1, 7, 8]), // start before StartDate
			CreateTestCase(new Date(2000, 6, 15), Tuesday | Thursday, [0, 5, 7]), // start on scheduled date
			CreateTestCase(new Date(2000, 6, 15), Sunday | Saturday, [2, 3, 9]), // start on unscheduled date
			CreateTestCase(new Date(2000, 12, 24), Sunday, [0, 7]), // start on scheduled date; end on EndDate
			CreateTestCase(new Date(2000, 12, 25), Sunday, [6]), // start on unscheduled date; end on EndDate
			CreateTestCase(new Date(2000, 12, 25), Monday, [0]), // start and end on EndDate
			CreateTestCase(new Date(2000, 12, 26), Monday, []), // start after EndDate
		];
	}

	#endregion

	private static Object[] CreateTestCase(Date date, DaysOfWeek daysOfWeek, Int32[] expectedDayOffsets)
	{
		Date[] expectedResults = Array.ConvertAll(expectedDayOffsets, date.AddDays);
		return [daysOfWeek, date, expectedResults];
	}
}
