using System;
using System.Collections.Generic;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="Schedule" /> class.
/// </summary>
[TestFixture]
public class ScheduleTests
{
	[Test]
	public void Contains_WithEmptyDate_ReturnsFalse()
	{
		// Arrange:
		Schedule schedule = new MockSchedule();
		Date date = Date.Empty;

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().BeFalse();
	}

	[Test]
	public void EnumerateBackwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new MockSchedule();
		Date date = Date.Empty;

		// Act:
		Func<IEnumerable<Date>> func = () => schedule.EnumerateBackwardFrom(date);

		// Assert:
		func.Should().Throw<ArgumentException>()
			.WithParameterName("date")
			.WithMessage("The value cannot be the default (empty) date.*");
	}

	[Test]
	public void EnumerateForwardFrom_WithEmptyDate_ThrowsException()
	{
		// Arrange:
		Schedule schedule = new MockSchedule();
		Date date = Date.Empty;

		// Act:
		Func<IEnumerable<Date>> func = () => schedule.EnumerateForwardFrom(date);

		// Assert:
		func.Should().Throw<ArgumentException>()
			.WithParameterName("date")
			.WithMessage("The value cannot be the default (empty) date.*");
	}
}
