using System;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="DayOfWeekAdjustments" /> class.
/// </summary>
[TestFixture]
public class DayOfWeekAdjustmentsTests
{
	[TestCase((DayOfWeek)(-1))]
	[TestCase((DayOfWeek)7)]
	public void GetItem_WithInvalidArgument_ThrowsException(DayOfWeek dayOfWeek)
	{
		// Arrange:
		DayOfWeekAdjustments adjustments = new();

		// Act:
		Func<Int32> func = () => adjustments[dayOfWeek];

		// Assert:
		func.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("dayOfWeek");
	}

	[TestCase((DayOfWeek)(-1))]
	[TestCase((DayOfWeek)7)]
	public void SetItem_WithInvalidArgument_ThrowsException(DayOfWeek dayOfWeek)
	{
		// Arrange:
		DayOfWeekAdjustments adjustments = new();

		// Act:
		Action action = () => adjustments[dayOfWeek] = 0;

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("dayOfWeek");
	}

	[TestCase(DayOfWeek.Sunday, -7)]
	[TestCase(DayOfWeek.Saturday, 7)]
	public void SetItem_WithInvalidValue_ThrowsException(DayOfWeek dayOfWeek, Int32 value)
	{
		// Arrange:
		DayOfWeekAdjustments adjustments = new();

		// Act:
		Action action = () => adjustments[dayOfWeek] = value;

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("value");
		adjustments[dayOfWeek].Should().Be(0);
	}

	[TestCase(DayOfWeek.Sunday, -6)]
	[TestCase(DayOfWeek.Wednesday, 0)]
	[TestCase(DayOfWeek.Saturday, 6)]
	public void SetItem_WithValidArgumentAndValue_UpdatesProperty(DayOfWeek dayOfWeek, Int32 value)
	{
		// Arrange:
		DayOfWeekAdjustments adjustments = new();

		// Act:
		adjustments[dayOfWeek] = value;

		// Assert:
		adjustments[dayOfWeek].Should().Be(value);
	}
}
