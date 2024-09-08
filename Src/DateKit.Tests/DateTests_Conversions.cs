using System;
using System.Globalization;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit;

partial class DateTests
{
#if NET6_0_OR_GREATER
	#region FromDateOnly

	[TestCase(Date.MinYear, 1, 1)]
	[TestCase(2000, 6, 15)]
	[TestCase(Date.MaxYear, 12, 31)]
	public void FromDateOnly_ReturnsExpected(Int32 year, Int32 month, Int32 day)
	{
		// Arrange:
		DateOnly dateOnly = new(year, month, day);

		// Act:
		Date actualDate = Date.FromDateOnly(dateOnly);

		// Assert:
		actualDate.Year.Should().Be(year);
		actualDate.Month.Should().Be(month);
		actualDate.Day.Should().Be(day);
	}

	#endregion
#endif // #if NET6_0_OR_GREATER

	#region FromDateTime

	[TestCase(Date.MinYear, 1, 1)]
	[TestCase(2000, 6, 15)]
	[TestCase(Date.MaxYear, 12, 31)]
	public void FromDateTime_ReturnsExpected(Int32 year, Int32 month, Int32 day)
	{
		// Arrange:
		DateTime dateTime = new(year, month, day);

		// Act:
		Date actualDate = Date.FromDateTime(dateTime);

		// Assert:
		actualDate.Year.Should().Be(year);
		actualDate.Month.Should().Be(month);
		actualDate.Day.Should().Be(day);
	}

	#endregion

	#region FromDayNumber

	[TestCase(-1)]
	[TestCase(3652059)]
	public void FromDayNumber_WithInvalidArgument_ThrowsException(Int32 dayNumber)
	{
		// Act:
		Func<Date> func = () => Date.FromDayNumber(dayNumber);

		// Assert:
		func.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("dayNumber");
	}

	[TestCase(0, 1, 1, 1)]
	[TestCase(730285, 2000, 6, 15)]
	[TestCase(3652058, 9999, 12, 31)]
	public void FromDayNumber_WithValidArgument_ReturnsExpected(
		Int32 dayNumber, Int32 expectedYear, Int32 expectedMonth, Int32 expectedDay)
	{
		// Act:
		Date actualDate = Date.FromDayNumber(dayNumber);

		// Assert:
		actualDate.Year.Should().Be(expectedYear);
		actualDate.Month.Should().Be(expectedMonth);
		actualDate.Day.Should().Be(expectedDay);
	}

	#endregion

	#region ParseIsoString

	[Test]
	public void ParseIsoString_WithNull_ThrowsException()
	{
		// Act:
		Func<Date> func = () => Date.ParseIsoString(null!);

		// Assert:
		func.Should().Throw<ArgumentNullException>().WithParameterName("s");
	}

	[TestCase("")]
	[TestCase("2000/06/15")]
	public void ParseIsoString_WithInvalidArgument_ThrowsException(String s)
	{
		// Act:
		Func<Date> func = () => Date.ParseIsoString(s);

		// Assert:
		func.Should().Throw<FormatException>();
	}

	[TestCase("2000-06-15", 2000, 6, 15)]
	public void ParseIsoString_WithValidArgument_ReturnsDate(
		String s, Int32 expectedYear, Int32 expectedMonth, Int32 expectedDay)
	{
		// Act:
		Date actualDate = Date.ParseIsoString(s);

		// Assert:
		actualDate.Year.Should().Be(expectedYear);
		actualDate.Month.Should().Be(expectedMonth);
		actualDate.Day.Should().Be(expectedDay);
	}

	#endregion

#if NET6_0_OR_GREATER
	#region ToDateOnly

	[Test]
	public void ToDateOnly_OfEmptyDate_ThrowsException()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		Func<DateOnly> func = () => date.ToDateOnly();

		// Arrange:
		func.Should().Throw<InvalidOperationException>();
	}

	[TestCase(Date.MinYear, 1, 1)]
	[TestCase(2000, 6, 15)]
	[TestCase(Date.MaxYear, 12, 31)]
	public void ToDateOnly_OfValidDate_ReturnsExpected(Int32 year, Int32 month, Int32 day)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		DateOnly actualDateOnly = date.ToDateOnly();

		// Assert:
		actualDateOnly.Year.Should().Be(year);
		actualDateOnly.Month.Should().Be(month);
		actualDateOnly.Day.Should().Be(day);
	}

	#endregion
#endif // #if NET6_0_OR_GREATER

	#region ToDateTime

	[Test]
	public void ToDateTime_OfEmptyDate_ThrowsException()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		Func<DateTime> func = () => date.ToDateTime();

		// Arrange:
		func.Should().Throw<InvalidOperationException>();
	}

	[TestCase(Date.MinYear, 1, 1, DateTimeKind.Unspecified)]
	[TestCase(2000, 6, 15, DateTimeKind.Utc)]
	[TestCase(Date.MaxYear, 12, 31, DateTimeKind.Local)]
	public void ToDateTime_OfValidDate_ReturnsExpected(Int32 year, Int32 month, Int32 day, DateTimeKind kind)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		DateTime actualDateTime = date.ToDateTime(kind);

		// Assert:
		actualDateTime.Year.Should().Be(year);
		actualDateTime.Month.Should().Be(month);
		actualDateTime.Day.Should().Be(day);
		actualDateTime.Kind.Should().Be(kind);
	}

	#endregion

	#region ToIsoString

	[Test]
	public void ToIsoString_OfEmptyDate_ReturnsEmptyString()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		String actualResult = date.ToIsoString();

		// Assert:
		actualResult.Should().BeEmpty();
	}

	[TestCase(1, 1, 1, "0001-01-01")]
	[TestCase(2000, 6, 15, "2000-06-15")]
	[TestCase(9999, 12, 31, "9999-12-31")]
	public void ToIsoString_OfValidDate_ReturnsExpected(Int32 year, Int32 month, Int32 day, String expectedResult)
	{
		// Arrange:
		Date date = new(year, month, day);

		// Act:
		String actualResult = date.ToIsoString();

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	#endregion

	#region ToString

	[Test]
	public void ToString_OfEmptyDate_ReturnsEmptyString()
	{
		// Arrange:
		Date date = Date.Empty;

		// Act:
		String actualResult = date.ToString();

		// Assert:
		actualResult.Should().BeEmpty();
	}

	[Test]
	public void ToString_WithInvalidArguments_ThrowsException()
	{
		// Arrange:
		Date date = new(2000, 6, 15);

		// Act:
		Func<String> func = () => date.ToString("G");

		// Assert:
		func.Should().Throw<FormatException>();
	}

	[TestCase(null, "", "06/15/2000")]
	[TestCase("D", "en-US", "Thursday, June 15, 2000")]
	public void ToString_WithValidArguments_ReturnsExpected(String format, String culture, String expectedResult)
	{
		// Arrange:
		Date date = new(2000, 6, 15);
		IFormatProvider provider = CultureInfo.GetCultureInfo(culture);

		// Act:
		String actualResult = date.ToString(format, provider);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	#endregion
}
