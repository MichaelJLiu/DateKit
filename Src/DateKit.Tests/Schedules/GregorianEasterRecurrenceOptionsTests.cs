using System;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="GregorianEasterRecurrenceOptions" /> class.
/// </summary>
[TestFixture]
public class GregorianEasterRecurrenceOptionsTests
{
	#region Offset

	[TestCase(-81)]
	[TestCase(251)]
	public void SetOffset_WithInvalidValue_ThrowsException(Int32 offset)
	{
		// Arrange:
		GregorianEasterRecurrenceOptions options = new();

		// Act:
		Action action = () => options.Offset = offset;

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("value");
		options.Offset.Should().Be(0);
	}

	[TestCase(-80)]
	[TestCase(0)]
	[TestCase(250)]
	public void SetOffset_WithValidValue_UpdatesProperty(Int32 offset)
	{
		// Arrange:
		GregorianEasterRecurrenceOptions options = new();

		// Act:
		options.Offset = offset;

		// Assert:
		options.Offset.Should().Be(offset);
	}

	#endregion
}
