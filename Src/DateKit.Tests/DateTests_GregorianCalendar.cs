using System;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit;

partial class DateTests
{
	#region DaysInMonth

	[TestCase(Date.MinYear - 1, 1, "year")]
	[TestCase(Date.MinYear, 0, "month")]
	[TestCase(Date.MaxYear, 13, "month")]
	[TestCase(Date.MaxYear + 1, 12, "year")]
	public void DaysInMonth_WithInvalidArguments_ThrowsException(Int32 year, Int32 month, String expectedParamName)
	{
		// Act:
		Action action = () => Date.DaysInMonth(year, month);

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(expectedParamName);
	}

	[TestCase(Date.MinYear, 1, 31)]
	[TestCase(2000, 1, 31)]
	[TestCase(2000, 2, 29)] // leap year
	[TestCase(2000, 3, 31)]
	[TestCase(2000, 4, 30)]
	[TestCase(2000, 5, 31)]
	[TestCase(2000, 6, 30)]
	[TestCase(2000, 7, 31)]
	[TestCase(2000, 8, 31)]
	[TestCase(2000, 9, 30)]
	[TestCase(2000, 10, 31)]
	[TestCase(2000, 11, 30)]
	[TestCase(2000, 12, 31)]
	[TestCase(2001, 2, 28)] // common year
	[TestCase(Date.MaxYear, 12, 31)]
	public void DaysInMonth_WithValidArguments_ReturnsExpected(Int32 year, Int32 month, Int32 expectedResult)
	{
		// Act:
		Int32 actualResult = Date.DaysInMonth(year, month);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	#endregion

	#region IsLeapYear

	[TestCase(Date.MinYear - 1)]
	[TestCase(Date.MaxYear + 1)]
	public void IsLeapYear_WithInvalidArgument_ThrowsException(Int32 year)
	{
		// Act:
		Action action = () => Date.IsLeapYear(year);

		// Assert:
		action.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("year");
	}

	[TestCase(1, false)]
	[TestCase(1900, false)] // common year (multiple of 100)
	[TestCase(2000, true)] // leap year (multiple of 400)
	[TestCase(2001, false)]
	[TestCase(2002, false)]
	[TestCase(2003, false)]
	[TestCase(2004, true)] // leap year (multiple of 4)
	[TestCase(9999, false)]
	public void IsLeapYear_WithValidArgument_ReturnsExpected(Int32 year, Boolean expectedResult)
	{
		// Act:
		Boolean actualResult = Date.IsLeapYear(year);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	[Test]
	public void IsLeapYear_ExhaustiveTest()
	{
		for (Int32 year = Date.MinYear; year <= Date.MaxYear; ++year)
		{
			// Act:
			Boolean actualResult = Date.IsLeapYear(year);

			// Assert:
			Boolean expectedResult = DateTime.IsLeapYear(year);
			actualResult.Should().Be(expectedResult);
		}
	}

	#endregion
}
