using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Moq;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="CompositeSchedule" /> class.
/// </summary>
[TestFixture]
public class CompositeScheduleTests
{
	#region Constructor

	[Test]
	public void Constructor_WithNull_ThrowsException()
	{
		// Arrange:
		Schedule[] baseSchedules = null!;

		// Act:
		Func<CompositeSchedule> func = () => new CompositeSchedule(baseSchedules);

		// Assert:
		func.Should().Throw<ArgumentNullException>().WithParameterName("baseSchedules");
	}

	#endregion

	#region BaseSchedules

	[Test]
	public void BaseSchedules_ReturnsExpected()
	{
		// Arrange:
		Schedule[] baseSchedules = Enumerable.Range(0, 3).Select(_ => Mock.Of<Schedule>()).ToArray();
		CompositeSchedule schedule = new(baseSchedules);

		// Act:
		IReadOnlyCollection<Schedule> actualBaseSchedules = schedule.BaseSchedules;

		// Assert:
		actualBaseSchedules.Should().Equal(baseSchedules);
	}

	#endregion

	#region Contains

	[TestCase(false, false)]
	[TestCase(false, true)]
	[TestCase(true, false)]
	[TestCase(true, true)]
	public void Contains_ReturnsExpected(Boolean isDateInBaseSchedule1, Boolean isDateInBaseSchedule2)
	{
		// Arrange:
		Date date = new(2000, 6, 15);
		Schedule schedule = new CompositeSchedule(
			Mock.Of<Schedule>(baseSchedule1 => baseSchedule1.Contains(date) == isDateInBaseSchedule1),
			Mock.Of<Schedule>(baseSchedule2 => baseSchedule2.Contains(date) == isDateInBaseSchedule2));

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(isDateInBaseSchedule1 || isDateInBaseSchedule2);
	}

	#endregion

	#region EnumerateBackwardFrom

	[Test]
	public void EnumerateBackwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new CompositeSchedule(Mock.Of<Schedule>());
		Date date = Date.Empty;

		// Act:
		Func<IEnumerable<Date>> func = () => schedule.EnumerateBackwardFrom(date);

		// Assert:
		func.Should().Throw<ArgumentException>().WithParameterName("date");
	}

	[Test]
	public void EnumerateBackwardFrom_WithValidDate_ReturnsExpected()
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		Schedule CreateBaseSchedule(params Date[] dates)
		{
			return Mock.Of<Schedule>(baseSchedule => baseSchedule.EnumerateBackwardFrom(date) == dates);
		}

		Date[] dates = Enumerable.Range(1, 5).Select(offset => date.AddDays(-offset)).ToArray();
		Schedule schedule = new CompositeSchedule(
			CreateBaseSchedule(dates[1]),
			CreateBaseSchedule(),
			CreateBaseSchedule(dates[2], dates[4]),
			CreateBaseSchedule(dates[0], dates[2], dates[3]));

		// Act:
		Date[] actualResults = schedule.EnumerateBackwardFrom(date).ToArray();

		// Assert:
		actualResults.Should().Equal(dates);
	}

	#endregion

	#region EnumerateForwardFrom

	[Test]
	public void EnumerateForwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new CompositeSchedule(Mock.Of<Schedule>());
		Date date = Date.Empty;

		// Act:
		Func<IEnumerable<Date>> func = () => schedule.EnumerateForwardFrom(date);

		// Assert:
		func.Should().Throw<ArgumentException>().WithParameterName("date");
	}

	[Test]
	public void EnumerateForwardFrom_WithValidDate_ReturnsExpected()
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		Schedule CreateBaseSchedule(params Date[] dates)
		{
			return Mock.Of<Schedule>(baseSchedule => baseSchedule.EnumerateForwardFrom(date) == dates);
		}

		Date[] dates = Enumerable.Range(1, 5).Select(date.AddDays).ToArray();
		Schedule schedule = new CompositeSchedule(
			CreateBaseSchedule(dates[1]),
			CreateBaseSchedule(),
			CreateBaseSchedule(dates[2], dates[4]),
			CreateBaseSchedule(dates[0], dates[2], dates[3]));

		// Act:
		Date[] actualResults = schedule.EnumerateForwardFrom(date).ToArray();

		// Assert:
		actualResults.Should().Equal(dates);
	}

	#endregion
}
