using System;
using System.Collections.Immutable;
using System.Linq;

using FluentAssertions;

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
	public void Constructor_WithDefault_ThrowsException()
	{
		// Arrange:
		ImmutableArray<Schedule> baseSchedules = default;

		// Act:
		Func<CompositeSchedule> func = () => new CompositeSchedule(baseSchedules);

		// Assert:
		func.Should().Throw<ArgumentNullException>().WithParameterName("baseSchedules");
	}

	[Test]
	public void Constructor_WithNullSchedule_ThrowsException()
	{
		// Arrange:
		ImmutableArray<Schedule> baseSchedules = [new MockSchedule(), null!, new MockSchedule()];

		// Act:
		Func<CompositeSchedule> func = () => new CompositeSchedule(baseSchedules);

		// Assert:
		func.Should().Throw<ArgumentException>()
			.WithParameterName("baseSchedules")
			.WithMessage("The collection cannot contain null references.*");
	}

	#endregion

	#region BaseSchedules

	[Test]
	public void BaseSchedules_ReturnsExpected()
	{
		// Arrange:
		ImmutableArray<Schedule> baseSchedules = [new MockSchedule(), new MockSchedule(), new MockSchedule()];
		CompositeSchedule schedule = new(baseSchedules);

		// Act:
		ImmutableArray<Schedule> actualBaseSchedules = schedule.BaseSchedules;

		// Assert:
		actualBaseSchedules.Should().Equal(baseSchedules);
	}

	#endregion

	#region Contains

	[Test]
	public void Contains_WithValidDate_ReturnsExpected(
		[Values(false, true)] Boolean isDateInBaseSchedule1,
		[Values(false, true)] Boolean isDateInBaseSchedule2)
	{
		// Arrange:
		Date date = new(2000, 6, 15);
		Schedule schedule = new CompositeSchedule(
			new MockSchedule(contains: [(date, isDateInBaseSchedule1)]),
			new MockSchedule(contains: [(date, isDateInBaseSchedule2)]));

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(isDateInBaseSchedule1 || isDateInBaseSchedule2);
	}

	#endregion

	#region EnumerateBackwardFrom

	[Test]
	public void EnumerateBackwardFrom_WithValidDate_ReturnsExpected()
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		Schedule CreateBaseSchedule(params Date[] dates)
		{
			return new MockSchedule(enumerateBackwardFrom: [(date, dates)]);
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
	public void EnumerateForwardFrom_WithValidDate_ReturnsExpected()
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		Schedule CreateBaseSchedule(params Date[] dates)
		{
			return new MockSchedule(enumerateForwardFrom: [(date, dates)]);
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
