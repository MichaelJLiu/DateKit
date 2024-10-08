using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Moq;

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
	public void Constructor_WithNull_ThrowsException()
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
		Schedule baseSchedule = Mock.Of<Schedule>();

		// Act:
		InverseSchedule schedule = new(baseSchedule);

		// Assert:
		schedule.BaseSchedule.Should().BeSameAs(baseSchedule);
	}

	#endregion

	#region Contains

	[Test]
	public void Contains_WithEmptyDate_ReturnsFalse()
	{
		// Arrange:
		Date date = Date.Empty;
		Schedule schedule = new InverseSchedule(
			Mock.Of<Schedule>(schedule => !schedule.Contains(date)));

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().BeFalse();
	}

	[TestCase(false)]
	[TestCase(true)]
	public void Contains_WithValidDate_ReturnsExpected(Boolean isDateInBaseSchedule)
	{
		// Arrange:
		Date date = new(2000, 6, 15);
		Schedule schedule = new InverseSchedule(
			Mock.Of<Schedule>(schedule => schedule.Contains(date) == isDateInBaseSchedule));

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(!isDateInBaseSchedule);
	}

	#endregion

	#region EnumerateBackwardFrom

	[Test]
	public void EnumerateBackwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new InverseSchedule(Mock.Of<Schedule>());
		Date date = Date.Empty;

		// Act:
		Func<IEnumerable<Date>> func = () => schedule.EnumerateBackwardFrom(date);

		// Assert:
		func.Should().Throw<ArgumentException>().WithParameterName("date");
	}

	[TestCaseSource(nameof(GetEnumerateBackwardFromTestCases))]
	public void EnumerateBackwardFrom_WithValidDate_ReturnsExpected(
		Date date, Date[] baseDates, Int32 takeCount, Date[] expectedDates)
	{
		// Arrange:
		Schedule schedule = new InverseSchedule(
			Mock.Of<Schedule>(schedule => schedule.EnumerateBackwardFrom(date) == baseDates));

		// Act:
		Date[] actualResults = schedule.EnumerateBackwardFrom(date).Take(takeCount).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedDates);
	}

	private static IEnumerable<Object[]> GetEnumerateBackwardFromTestCases()
	{
		return
		[
			CreateTestCase(new Date(2000, 6, 15), [], 3, [0, -1, -2]),
			CreateTestCase(new Date(2000, 6, 15), [-1, -4, -5], 4, [0, -2, -3, -6]),
			CreateTestCase(Date.MinValue, [], 2, [0]),
			CreateTestCase(Date.MinValue, [0], 1, [])
		];
	}

	#endregion

	#region EnumerateForwardFrom

	[Test]
	public void EnumerateForwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new InverseSchedule(Mock.Of<Schedule>());
		Date date = Date.Empty;

		// Act:
		Func<IEnumerable<Date>> func = () => schedule.EnumerateForwardFrom(date);

		// Assert:
		func.Should().Throw<ArgumentException>().WithParameterName("date");
	}

	[TestCaseSource(nameof(GetEnumerateForwardFromTestCases))]
	public void EnumerateForwardFrom_WithValidDate_ReturnsExpected(
		Date date, Date[] baseDates, Int32 takeCount, Date[] expectedDates)
	{
		// Arrange:
		Schedule schedule = new InverseSchedule(
			Mock.Of<Schedule>(schedule => schedule.EnumerateForwardFrom(date) == baseDates));

		// Act:
		Date[] actualResults = schedule.EnumerateForwardFrom(date).Take(takeCount).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedDates);
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
		return
		[
			date,
			Array.ConvertAll(baseDateOffsets, date.AddDays),
			takeCount,
			Array.ConvertAll(expectedDateOffsets, date.AddDays),
		];
	}
}
