using System;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit;

partial class DateTests
{
	#region AddYears

	[TestCase(Int32.MinValue)]
	[TestCase(Date.MinYear - Date.MaxYear - 1)]
	[TestCase(Date.MaxYear - Date.MinYear + 1)]
	[TestCase(Int32.MaxValue)]
	public void AddYears_WithInvalidArgument_ThrowsException(Int32 number)
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		// Act:
		Func<Date> func = () => date.AddYears(number);

		// Assert:
		func.Should().Throw<OverflowException>();
	}

	[Test]
	public void AddYears_OfEmptyDate_ThrowsException()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		Func<Date> func = () => date.AddYears(0);

		// Assert:
		func.Should().Throw<InvalidOperationException>();
	}

	[TestCase(Date.MinYear, 1, 1, -1)]
	[TestCase(2000, 6, 15, Date.MinYear - 2000 - 1)]
	[TestCase(2000, 6, 15, Date.MaxYear - 2000 + 1)]
	[TestCase(Date.MaxYear, 12, 31, 1)]
	public void AddYears_WhenResultOverflows_ThrowsException(Int32 year, Int32 month, Int32 day, Int32 number)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		Func<Date> func = () => date.AddYears(number);

		// Assert:
		func.Should().Throw<OverflowException>();
	}

	[TestCase(Date.MinYear, 12, 31, Date.MaxYear - Date.MinYear)]
	[TestCase(2000, 2, 28, 1)]
	[TestCase(2000, 2, 29, 1)] // leap day to non-leap day
	[TestCase(2000, 2, 29, 4)] // leap day to leap day
	[TestCase(2000, 6, 15, Date.MinYear - 2000)]
	[TestCase(2000, 6, 15, Date.MaxYear - 2000)]
	[TestCase(Date.MaxYear, 1, 1, Date.MinYear - Date.MaxYear)]
	public void AddYears_WhenResultDoesNotOverflow_ReturnsExpected(Int32 year, Int32 month, Int32 day, Int32 number)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		Date actualDate = date.AddYears(number);

		// Assert:
		DateTime expectedDate = new DateTime(year, month, day).AddYears(number);
		actualDate.Year.Should().Be(expectedDate.Year);
		actualDate.Month.Should().Be(expectedDate.Month);
		actualDate.Day.Should().Be(expectedDate.Day);
	}

	#endregion

	#region AddMonths

	[TestCase(Int32.MinValue)]
	[TestCase((Date.MinYear - Date.MaxYear - 1) * Date.MonthsPerYear)]
	[TestCase((Date.MaxYear - Date.MinYear + 1) * Date.MonthsPerYear)]
	[TestCase(Int32.MaxValue)]
	public void AddMonths_WithInvalidArgument_ThrowsException(Int32 number)
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		// Act:
		Func<Date> func = () => date.AddMonths(number);

		// Assert:
		func.Should().Throw<OverflowException>();
	}

	[Test]
	public void AddMonths_OfEmptyDate_ThrowsException()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		Func<Date> func = () => date.AddMonths(0);

		// Assert:
		func.Should().Throw<InvalidOperationException>();
	}

	[TestCase(Date.MinYear, 1, 1, -1)]
	[TestCase(2000, 6, 15, -((2000 - Date.MinYear) * Date.MonthsPerYear + 6))]
	[TestCase(2000, 6, 15, (Date.MaxYear - 2000) * Date.MonthsPerYear + 7)]
	[TestCase(Date.MaxYear, 12, 31, 1)]
	public void AddMonths_WhenResultOverflows_ThrowsException(Int32 year, Int32 month, Int32 day, Int32 number)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		Func<Date> func = () => date.AddMonths(number);

		// Assert:
		func.Should().Throw<OverflowException>();
	}

	[TestCase(Date.MinYear, 1, 31, (Date.MaxYear - Date.MinYear + 1) * Date.MonthsPerYear - 1)]
	[TestCase(2000, 1, 31, 1)] // -> 2000-02-29
	[TestCase(2000, 1, 31, 2)] // -> 2000-03-31
	[TestCase(2000, 6, 15, -((2000 - Date.MinYear) * Date.MonthsPerYear + 5))]
	[TestCase(2000, 6, 15, -18)] // -> 1998-12-15
	[TestCase(2000, 6, 15, -17)] // -> 1999-01-15
	[TestCase(2000, 6, 15, -6)] // -> 1999-12-15
	[TestCase(2000, 6, 15, -5)] // -> 2000-01-15
	[TestCase(2000, 6, 15, 6)] // -> 2000-12-15
	[TestCase(2000, 6, 15, 7)] // -> 2001-01-15
	[TestCase(2000, 6, 15, (Date.MaxYear - 2000) * Date.MonthsPerYear + 6)]
	[TestCase(Date.MaxYear, 12, 1, (Date.MinYear - Date.MaxYear - 1) * Date.MonthsPerYear + 1)]
	public void AddMonths_WhenResultDoesNotOverflow_ReturnsExpected(Int32 year, Int32 month, Int32 day, Int32 number)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		Date actualDate = date.AddMonths(number);

		// Assert:
		DateTime expectedDate = new DateTime(year, month, day).AddMonths(number);
		actualDate.Year.Should().Be(expectedDate.Year);
		actualDate.Month.Should().Be(expectedDate.Month);
		actualDate.Day.Should().Be(expectedDate.Day);
	}

	#endregion

	#region AddDays

	[TestCase(Int32.MinValue)]
	[TestCase(-3652059)]
	[TestCase(3652059)]
	[TestCase(Int32.MaxValue)]
	public void AddDays_WithInvalidArgument_ThrowsException(Int32 number)
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		// Act:
		Func<Date> func = () => date.AddDays(number);

		// Assert:
		func.Should().Throw<OverflowException>();
	}

	// AddLargeNegativeDays
	[TestCase(-3652059)]
	[TestCase(-29)]
	// AddSmallNegativeDays
	[TestCase(-1)]
	// AddSmallPositiveDays
	[TestCase(0)]
	[TestCase(1)]
	// AddLargePositiveDays
	[TestCase(29)]
	[TestCase(3652059)]
	public void AddDays_OfEmptyDate_ThrowsException(Int32 number)
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		Func<Date> func = () => date.AddDays(number);

		// Assert:
		func.Should().Throw<InvalidOperationException>();
	}

	// AddLargeNegativeDays
	[TestCase(Date.MinYear, 1, 29, -29)]
	[TestCase(2000, 6, 15, -730286)]
	// AddSmallNegativeDays
	[TestCase(Date.MinYear, 1, 1, -1)]
	// AddSmallPositiveDays
	[TestCase(Date.MaxYear, 12, 31, 1)]
	// AddLargePositiveDays
	[TestCase(2000, 6, 15, 2921774)]
	[TestCase(Date.MaxYear, 12, 3, 29)]
	public void AddDays_WhenResultOverflows_ThrowsException(Int32 year, Int32 month, Int32 day, Int32 number)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		Func<Date> func = () => date.AddDays(number);

		// Assert:
		func.Should().Throw<OverflowException>();
	}

	// AddLargeNegativeDays
	[TestCase(2000, 3, 1, -61)] // -> 1999-12-31 (year rollover)
	[TestCase(2000, 6, 15, -730285)] // -> 0001-01-01
	[TestCase(2000, 6, 15, -46)] // -> 2000-04-30
	[TestCase(2000, 6, 15, -29)] // -> 2000-05-17
	[TestCase(Date.MaxYear, 12, 31, -3652058)] // -> 0001-01-01
	// AddSmallNegativeDays
	[TestCase(Date.MinYear, 1, 2, -1)] // -> 0001-01-01
	[TestCase(2000, 1, 1, -1)] // -> 1999-12-31 (year rollover)
	[TestCase(2000, 2, 1, -1)] // -> 2000-01-31
	[TestCase(2000, 6, 15, -28)] // -> 2000-05-18
	[TestCase(2000, 6, 15, -15)] // -> 2000-05-31
	[TestCase(2000, 6, 15, -14)] // -> 2000-06-01
	[TestCase(2000, 6, 15, -1)]
	// AddSmallPositiveDays
	[TestCase(2000, 6, 15, 0)]
	[TestCase(2000, 6, 15, 1)]
	[TestCase(2000, 6, 15, 15)] // -> 2000-06-30
	[TestCase(2000, 6, 15, 16)] // -> 2000-07-01
	[TestCase(2000, 6, 15, 28)] // -> 2000-07-13
	[TestCase(2000, 11, 30, 1)] // -> 2000-12-01
	[TestCase(2000, 12, 31, 1)] // -> 2001-01-01 (year rollover)
	[TestCase(Date.MaxYear, 12, 30, 1)] // -> 9999-12-31
	// AddLargePositiveDays
	[TestCase(Date.MinYear, 1, 1, 3652058)] // -> 9999-12-31
	[TestCase(2000, 6, 15, 29)] // -> 2000-07-14
	[TestCase(2000, 6, 15, 46)] // -> 2000-07-31 (day > MinDaysPerMonth && day <= daysInMonth)
	[TestCase(2000, 6, 15, 47)] // -> 2000-08-01
	[TestCase(2000, 6, 15, 2921773)] // -> 9999-12-31
	[TestCase(2000, 12, 31, 32)] // -> 2001-02-01 (year rollover)
	[TestCase(2000, 12, 31, 60)] // -> 2001-03-01 (leap year to common year)
	public void AddDays_WhenResultDoesNotOverflow_ReturnsExpected(Int32 year, Int32 month, Int32 day, Int32 number)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		Date actualResult = date.AddDays(number);

		// Assert:
		DateTime expectedResult = new DateTime(year, month, day).AddDays(number);
		actualResult.Year.Should().Be(expectedResult.Year);
		actualResult.Month.Should().Be(expectedResult.Month);
		actualResult.Day.Should().Be(expectedResult.Day);
	}

	#endregion

	#region Subtract

	[Test]
	public void Subtract_WithInvalidArguments_ThrowsException()
	{
		// Arrange:
		Date date1 = Date.Empty;
		Date date2 = new(2000, 6, 15);

		// Act:
		Func<Int32> func1 = () => date1 - date2;
		Func<Int32> func2 = () => date2 - date1;

		// Assert:
		func1.Should().Throw<ArgumentException>().WithParameterName("date1");
		func2.Should().Throw<ArgumentException>().WithParameterName("date2");
	}

	[TestCase(1, 1, 1, -730285)]
	[TestCase(1999, 7, 16, -335)] // earlier year
	[TestCase(2000, 5, 16, -30)] // earlier month
	[TestCase(2000, 6, 14, -1)] // earlier day
	[TestCase(2000, 6, 15, 0)] // same date
	[TestCase(2000, 6, 16, 1)] // later day
	[TestCase(2000, 7, 14, 29)] // later month
	[TestCase(2001, 5, 14, 333)] // later year
	[TestCase(9999, 12, 31, 2921773)]
	public void Subtract_WithValidArguments_ReturnsExpected(Int32 year, Int32 month, Int32 day, Int32 expectedResult)
	{
		// Arrange:
		Date date1 = new(year, month, day);
		Date date2 = new(2000, 6, 15);

		// Act:
		Int32 actualResult1 = date1 - date2;
		Int32 actualResult2 = date2 - date1;

		// Assert:
		actualResult1.Should().Be(expectedResult);
		actualResult2.Should().Be(-expectedResult);
	}

	#endregion

	#region Decrement

	[Test]
	public void Decrement_OfEmptyDate_ThrowsException()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		Func<Date> func = () => --date;

		// Assert:
		func.Should().Throw<InvalidOperationException>();
	}

	[Test]
	public void Decrement_WhenResultOverflows_ThrowsException()
	{
		// Arrange:
		Date date = Date.MinValue;

		// Act:
		Func<Date> func = () => --date;

		// Assert:
		func.Should().Throw<OverflowException>();
	}

	[TestCase(Date.MinYear, 1, 2)]
	[TestCase(2000, 1, 1)]
	[TestCase(2000, 6, 1)]
	[TestCase(Date.MaxYear, 12, 31)]
	public void Decrement_WhenResultDoesNotOverflow_ReturnsExpected(Int32 year, Int32 month, Int32 day)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		--date;

		// Assert:
		DateTime expectedResult = new DateTime(year, month, day).Add(-OneDay);
		date.Year.Should().Be(expectedResult.Year);
		date.Month.Should().Be(expectedResult.Month);
		date.Day.Should().Be(expectedResult.Day);
	}

	#endregion

	#region Increment

	[Test]
	public void Increment_OfEmptyDate_ThrowsException()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		Func<Date> func = () => ++date;

		// Assert:
		func.Should().Throw<InvalidOperationException>();
	}

	[Test]
	public void Increment_WhenResultOverflows_ThrowsException()
	{
		// Arrange:
		Date date = Date.MaxValue;

		// Act:
		Func<Date> func = () => ++date;

		// Assert:
		func.Should().Throw<OverflowException>();
	}

	[TestCase(Date.MinYear, 1, 1)]
	[TestCase(2000, 6, 30)]
	[TestCase(2000, 12, 31)]
	[TestCase(Date.MaxYear, 12, 30)]
	public void Increment_WhenResultDoesNotOverflow_ReturnsExpected(Int32 year, Int32 month, Int32 day)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		++date;

		// Assert:
		DateTime expectedResult = new DateTime(year, month, day).Add(OneDay);
		date.Year.Should().Be(expectedResult.Year);
		date.Month.Should().Be(expectedResult.Month);
		date.Day.Should().Be(expectedResult.Day);
	}

	#endregion
}
