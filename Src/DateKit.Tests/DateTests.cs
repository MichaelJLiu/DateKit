using System;
using System.Reflection;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit;

/// <summary>
/// Tests the <see cref="Date" /> structure.
/// </summary>
[TestFixture]
public partial class DateTests
{
	private static readonly TimeSpan OneDay = TimeSpan.FromTicks(TimeSpan.TicksPerDay);

	#region Constructor

	[TestCase(Date.MinYear - 1, 1, 1, "year")]
	[TestCase(Date.MinYear, 0, 1, "month")]
	[TestCase(Date.MinYear, 1, 0, "day")]
	[TestCase(2000, 2, 30, "day")]
	[TestCase(2000, 6, 31, "day")]
	[TestCase(Date.MaxYear, 12, 32, "day")]
	[TestCase(Date.MaxYear, 13, 31, "month")]
	[TestCase(Date.MaxYear + 1, 12, 31, "year")]
	public void Constructor_WithInvalidYearMonthDay_ThrowsException(
		Int32 year, Int32 month, Int32 day, String expectedParamName)
	{
		// Act:
		Func<Date> func = () => new Date(year, month, day);

		// Assert:
		func.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(expectedParamName);
	}

	[TestCase(Date.MinYear, 1, 1)]
	[TestCase(2000, 2, 29)]
	[TestCase(2000, 6, 15)]
	[TestCase(Date.MaxYear, 12, 31)]
	public void Constructor_WithValidYearMonthDay_ReturnsDate(Int32 year, Int32 month, Int32 day)
	{
		// Act:
		Date date = new(year, month, day);

		// Assert:
		date.Year.Should().Be(year);
		date.Month.Should().Be(month);
		date.Day.Should().Be(day);
	}

	#endregion

	#region Deconstruct

	[TestCase(2000, 6, 15)]
	public void Deconstruct_ReturnsDateComponents(Int32 year, Int32 month, Int32 day)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		date.Deconstruct(out Int32 actualYear, out Int32 actualMonth, out Int32 actualDay);

		// Assert:
		actualYear.Should().Be(year);
		actualMonth.Should().Be(month);
		actualDay.Should().Be(day);
	}

	#endregion

	#region DayNumber

	[Test]
	public void DayNumber_OfEmptyDate_ThrowsException()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		Func<Int32> func = () => date.DayNumber;

		// Assert:
		func.Should().Throw<InvalidOperationException>();
	}

	[TestCase(1, 1, 1, 0)]
	[TestCase(2000, 2, 29, 730178)] // last day of modified year
	[TestCase(2000, 3, 1, 730179)] // first day of modified year
	[TestCase(2000, 6, 15, 730285)]
	[TestCase(9999, 12, 31, 3652058)]
	public void DayNumber_OfValidDate_ReturnsExpected(Int32 year, Int32 month, Int32 day, Int32 expectedDayNumber)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		Int32 actualDayNumber = date.DayNumber;

		// Assert:
		actualDayNumber.Should().Be(expectedDayNumber);
	}

	#endregion

	#region DayOfWeek

	[Test]
	public void DayOfWeek_OfEmptyDate_ThrowsException()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		Func<DayOfWeek> func = () => date.DayOfWeek;

		// Assert:
		func.Should().Throw<InvalidOperationException>();
	}

	[TestCase(1, 1, 1, DayOfWeek.Monday)]
	[TestCase(2000, 1, 1, DayOfWeek.Saturday)]
	[TestCase(2000, 6, 15, DayOfWeek.Thursday)]
	[TestCase(2000, 12, 31, DayOfWeek.Sunday)]
	[TestCase(9999, 12, 31, DayOfWeek.Friday)]
	public void DayOfWeek_OfValidDate_ReturnsExpected(
		Int32 year, Int32 month, Int32 day, DayOfWeek expectedDayOfWeek)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		DayOfWeek actualDayOfWeek = date.DayOfWeek;

		// Assert:
		actualDayOfWeek.Should().Be(expectedDayOfWeek);
	}

	#endregion

	#region DayOfYear

	[Test]
	public void DayOfYear_OfEmptyDate_ThrowsException()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		Func<Int32> func = () => date.DayOfYear;

		// Assert:
		func.Should().Throw<InvalidOperationException>();
	}

	[TestCase(2000, 1, 1, 1)]
	[TestCase(2000, 2, 29, 60)]
	[TestCase(2000, 3, 1, 61)]
	[TestCase(2000, 6, 15, 167)]
	[TestCase(2000, 12, 31, 366)]
	[TestCase(2001, 3, 1, 60)]
	[TestCase(2001, 6, 15, 166)]
	public void DayOfYear_OfValidDate_ReturnsExpected(
		Int32 year, Int32 month, Int32 day, Int32 expectedDayOfYear)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		Int32 actualDayOfYear = date.DayOfYear;

		// Assert:
		actualDayOfYear.Should().Be(expectedDayOfYear);
	}

	#endregion

	#region DebuggerDisplay

	[Test]
	public void DebuggerDisplay_OfValidDate_ReturnsExpected()
	{
		// Arrange:
		PropertyInfo debuggerDisplayProperty =
			typeof(Date).GetProperty("DebuggerDisplay", BindingFlags.Instance | BindingFlags.NonPublic)!;
		Date date = new(2000, 6, 15);

		// Act:
		Object? actualResult = debuggerDisplayProperty.GetValue(date);

		// Assert:
		actualResult.Should().Be("2000-06-15");
	}

	#endregion

	/// <summary>
	/// Tests the <see cref="Date.FromDateTime" /> method as well as the <see cref="Date.DayNumber" />,
	/// <see cref="Date.DayOfWeek" />, and <see cref="Date.DayOfYear" /> properties for all possible dates.
	/// </summary>
	[Test]
	[Explicit("This test is slow. Run it on demand.")]
	public void ExhaustiveTest()
	{
		DateTime dateTime = new(Date.MinYear, 1, 1);
		Int32 expectedDayNumber = 0;
		DateTime maxValue = new(Date.MaxYear, 12, 31);

		while (true)
		{
			// Act:
			Date date = Date.FromDateTime(dateTime);
			Int32 actualDayNumber = date.DayNumber;
			DayOfWeek actualDayOfWeek = date.DayOfWeek;
			Int32 actualDayOfYear = date.DayOfYear;

			// Assert:
			date.Year.Should().Be(dateTime.Year);
			date.Month.Should().Be(dateTime.Month);
			date.Day.Should().Be(dateTime.Day);
			actualDayNumber.Should().Be(expectedDayNumber);
			actualDayOfWeek.Should().Be(dateTime.DayOfWeek);
			actualDayOfYear.Should().Be(dateTime.DayOfYear);

			if (dateTime == maxValue)
				break;
			dateTime += OneDay;
			++expectedDayNumber;
		}
	}
}
