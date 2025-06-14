using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="InverseSchedule" /> class.
/// </summary>
[TestFixture]
public class InverseScheduleTests
{
	#region Constructor

	[Test]
	public void Constructor_WithNullSchedule_ThrowsException()
	{
		// Arrange:
		Schedule baseSchedule = null!;

		// Act:
		Func<InverseSchedule> func = () => new InverseSchedule(baseSchedule);

		// Assert:
		func.Should().Throw<ArgumentNullException>().WithParameterName("baseSchedule");
	}

	#endregion

	#region BaseSchedule

	[Test]
	public void BaseSchedule_ReturnsExpected()
	{
		// Arrange:
		Schedule baseSchedule = new MockSchedule();
		InverseSchedule schedule = new(baseSchedule);

		// Act:
		Schedule actualBaseSchedule = schedule.BaseSchedule;

		// Assert:
		actualBaseSchedule.Should().BeSameAs(baseSchedule);
	}

	#endregion

	#region Contains

	[TestCase(false)]
	[TestCase(true)]
	public void Contains_WithValidDate_ReturnsExpected(Boolean isDateInBaseSchedule)
	{
		// Arrange:
		Date date = new(2000, 6, 15);
		Schedule schedule = new InverseSchedule(new MockSchedule(contains: [(date, isDateInBaseSchedule)]));

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(!isDateInBaseSchedule);
	}

	#endregion

	#region EnumerateBackwardFrom

	[TestCaseSource(nameof(GetEnumerateBackwardFromTestCases))]
	public void EnumerateBackwardFrom_WithValidDate_ReturnsExpected(
		Date date, Date[] baseDates, Int32 takeCount, Date[] expectedResults)
	{
		// Arrange:
		Schedule schedule = new InverseSchedule(new MockSchedule(enumerateBackwardFrom: [(date, baseDates)]));

		// Act:
		Date[] actualResults = schedule.EnumerateBackwardFrom(date).Take(takeCount).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateBackwardFromTestCases()
	{
		return
		[
			CreateTestCase(new Date(2000, 6, 15), [], 3, [0, -1, -2]),
			CreateTestCase(new Date(2000, 6, 15), [-1, -4, -5], 4, [0, -2, -3, -6]),
			CreateTestCase(Date.MinValue, [], 2, [0]),
			CreateTestCase(Date.MinValue, [0], 1, []),
		];
	}

	#endregion

	#region EnumerateForwardFrom

	[TestCaseSource(nameof(GetEnumerateForwardFromTestCases))]
	public void EnumerateForwardFrom_WithValidDate_ReturnsExpected(
		Date date, Date[] baseDates, Int32 takeCount, Date[] expectedResults)
	{
		// Arrange:
		Schedule schedule = new InverseSchedule(new MockSchedule(enumerateForwardFrom: [(date, baseDates)]));

		// Act:
		Date[] actualResults = schedule.EnumerateForwardFrom(date).Take(takeCount).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateForwardFromTestCases()
	{
		return
		[
			CreateTestCase(new Date(2000, 6, 15), [], 3, [0, 1, 2]),
			CreateTestCase(new Date(2000, 6, 15), [1, 4, 5], 4, [0, 2, 3, 6]),
			CreateTestCase(Date.MaxValue, [], 2, [0]),
			CreateTestCase(Date.MaxValue, [0], 1, []),
		];
	}

	#endregion

	private static Object[] CreateTestCase(
		Date date, Int32[] baseDateOffsets, Int32 takeCount, Int32[] expectedDateOffsets)
	{
		Date[] baseDates = Array.ConvertAll(baseDateOffsets, date.AddDays);
		Date[] expectedDates = Array.ConvertAll(expectedDateOffsets, date.AddDays);
		return [date, baseDates, takeCount, expectedDates];
	}
}
