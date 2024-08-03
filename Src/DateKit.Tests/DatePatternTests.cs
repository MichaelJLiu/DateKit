using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit;

[TestFixture]
public class DatePatternTests
{
	#region Create

	[TestCase("A", "'A' is not a valid standard date format string.")]
	[TestCase("ABC", "Unexpected character 'A' at index 0.")]
	[TestCase("abc", "Unexpected character 'a' at index 0.")]
	[TestCase("ddddd", "Unexpected character 'd' at index 4.")]
	[TestCase("ggg", "Unexpected character 'g' at index 2.")]
	[TestCase("MMMMM", "Unexpected character 'M' at index 4.")]
	[TestCase("yyy", "Unexpected character 'y' at index 2.")]
	[TestCase("yyyyy", "Unexpected character 'y' at index 4.")]
	[TestCase(" '", "Unexpected end of string.")]
	[TestCase(" '\\", "Unexpected end of string.")]
	[TestCase(" \"", "Unexpected end of string.")]
	[TestCase(" \"\\", "Unexpected end of string.")]
	[TestCase(" \\", "Unexpected end of string.")]
	[TestCase(" %", "Unexpected character '%' at index 1.")]
	[TestCase("%%", "Unexpected character '%' at index 1.")]
	public void Create_WithInvalidFormat_ThrowsException(String format, String expectedMessage)
	{
		// Act:
		Func<DatePattern> func = () => DatePattern.Create(format);

		// Assert:
		func.Should().Throw<FormatException>().WithMessage(expectedMessage);
	}

	[Test]
	public void Create_WithUnsupportedCalendar_ThrowsException()
	{
		// Arrange:
		CultureInfo culture = CultureInfo.GetCultureInfo("ar-SA");

		// Act:
		Func<DatePattern> func = () => DatePattern.Create(format: null, culture);

		// Assert:
		func.Should().Throw<ArgumentException>()
			.WithMessage($"{nameof(UmAlQuraCalendar)} is not supported.*")
			.WithParameterName("provider");
	}

	#endregion

	#region Format

	[Test]
	public void Format_WithEmptyDate_ReturnsEmptyString()
	{
		// Arrange:
		DatePattern pattern = DatePattern.Create("o");
		Date date = Date.Empty;

		// Act:
		String actualResult = pattern.Format(date);

		// Assert:
		actualResult.Should().BeEmpty();
	}

	// Standard format strings
	[TestCase(null, 2000, 6, 15, "06/15/2000")]
	[TestCase("", 2000, 6, 15, "06/15/2000")]
	[TestCase("d", 2000, 6, 15, "06/15/2000")]
	[TestCase("D", 2000, 6, 15, "Thursday, 15 June 2000")]
	[TestCase("m", 2000, 6, 15, "June 15")]
	[TestCase("M", 2000, 6, 15, "June 15")]
	[TestCase("y", 2000, 6, 15, "2000 June")]
	[TestCase("Y", 2000, 6, 15, "2000 June")]
	[TestCase("o", 2000, 6, 15, "2000-06-15")]
	[TestCase("O", 2000, 6, 15, "2000-06-15")]
	[TestCase("r", 2000, 6, 15, "Thu, 15 Jun 2000")]
	[TestCase("R", 2000, 6, 15, "Thu, 15 Jun 2000")]
	// Custom format strings
	[TestCase("%d", 1, 1, 1, "1")]
	[TestCase("dd", 1, 1, 1, "01")]
	[TestCase("ddd", 1, 1, 1, "Mon")]
	[TestCase("dddd", 1, 1, 1, "Monday")]
	[TestCase("%d", 9999, 12, 31, "31")]
	[TestCase("d/", 9999, 12, 31, "31/")]
	[TestCase("dd/", 9999, 12, 31, "31/")]
	[TestCase("ddd/", 9999, 12, 31, "Fri/")]
	[TestCase("dddd/", 9999, 12, 31, "Friday/")]
	[TestCase("%M", 1, 1, 1, "1")]
	[TestCase("MM", 1, 1, 1, "01")]
	[TestCase("MMM", 1, 1, 1, "Jan")]
	[TestCase("MMMM", 1, 1, 1, "January")]
	[TestCase("%M", 9999, 12, 31, "12")]
	[TestCase("M/", 9999, 12, 31, "12/")]
	[TestCase("MM/", 9999, 12, 31, "12/")]
	[TestCase("MMM/", 9999, 12, 31, "Dec/")]
	[TestCase("MMMM/", 9999, 12, 31, "December/")]
	[TestCase("%y", 1, 1, 1, "1")]
	[TestCase("y ", 1, 1, 1, "1 ")]
	[TestCase("yy", 1, 1, 1, "01")]
	[TestCase("yyyy", 1, 1, 1, "0001")]
	[TestCase("yyyy", 99, 1, 1, "0099")]
	[TestCase("%y", 9999, 12, 31, "9999")]
	[TestCase("y/", 9999, 12, 31, "9999/")]
	[TestCase("yy/", 9999, 12, 31, "99/")]
	[TestCase("yyyy/", 100, 12, 31, "0100/")]
	[TestCase("yyyy/", 9999, 12, 31, "9999/")]
	[TestCase("%g", 1, 1, 1, "A.D.")]
	[TestCase("g ", 1, 1, 1, "A.D. ")]
	[TestCase("gg", 1, 1, 1, "A.D.")]
	[TestCase("%g", 9999, 12, 31, "A.D.")]
	[TestCase("g/", 9999, 12, 31, "A.D./")]
	[TestCase("gg/", 9999, 12, 31, "A.D./")]
	[TestCase("M/d/y", 2000, 6, 15, "6/15/2000")]
	[TestCase("yyyy-MM-dd", 2000, 6, 15, "2000-06-15")]
	[TestCase("ddd, dd MMM yyyy", 2000, 6, 15, "Thu, 15 Jun 2000")]
	[TestCase("dddd, dd MMMM yyyy", 2000, 6, 15, "Thursday, 15 June 2000")]
	[TestCase("<''\"\"\\d=\\'d\\\" 'M=\\''M'\"' \"y='\"y\"\\\"\" g>", 2000, 6, 15, "<d='15\" M='6\" y='2000\" A.D.>")]
	public void Format_WithInvariantCulture_ReturnsExpected(
		String? format, Int32 year, Int32 month, Int32 day, String expectedResult)
	{
		// Arrange:
		DatePattern pattern = DatePattern.Create(format, CultureInfo.InvariantCulture);
		Date date = new(year, month, day);

		// Act:
		String actualResult = pattern.Format(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	/// <summary>
	/// Verifies that <see cref="DatePattern.Format" /> and <see cref="DateTime.ToString(String, IFormatProvider)" />
	/// return the same result for every standard date format of every supported culture.
	/// </summary>
	[TestCaseSource(nameof(GetSpecificCultureTestCases))]
	public void Format_WithSpecificCulture_ReturnsExpected(CultureInfo culture, String format)
	{
		// Arrange:
		DatePattern pattern = DatePattern.Create(format, culture);
		Date date = new(2000, 6, 15);

		// Act:
		String actualResult = pattern.Format(date);

		// Assert:
		String expectedResult = new DateTime(2000, 6, 15).ToString(format, culture);
		actualResult.Should().Be(expectedResult);
	}

	private static IEnumerable<Object[]> GetSpecificCultureTestCases()
	{
		Boolean runtimeSupportsAbbreviatedMonthGenitiveName = Environment.Version.ToString(3) != "4.0.30319";

		return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
			.Where(culture => culture.Calendar is GregorianCalendar)
			.SelectMany(
				culture =>
				{
					DateTimeFormatInfo info = culture.DateTimeFormat;
					return new Object[][]
					{
						[culture, info.ShortDatePattern],
						[
							culture,
							// Before .NET Core 3.0, if LongDatePattern contains the "MMM" specifier, then
							// DateTime.ToString incorrectly uses the abbreviated regular month name instead of
							// the abbreviated genitive month name. To avoid false positives, replace the specifier:
							runtimeSupportsAbbreviatedMonthGenitiveName
								? info.LongDatePattern
								: info.LongDatePattern.Replace(" MMM ", " MMMM "),
						],
						[culture, info.MonthDayPattern],
						[
							culture,
							// In all .NET versions, if YearMonthPattern contains the literal "'de'",
							// then DateTime.ToString misinterprets the "d" as a day specifier and incorrectly uses
							// the genitive month name. To avoid false positives, replace the letter "d":
							info.YearMonthPattern.Replace("de", "De"),
						],
					};
				});
	}

	#endregion

	#region TryParseExact

	[TestCase("%d", "")] // insufficient digits
	[TestCase("%d", ":")] // invalid digit
	[TestCase("%d", "0")] // out of range
	[TestCase("%d", "32")] // out of range
	[TestCase("dd", "1")] // insufficient digits
	[TestCase("dd", "00")] // out of range
	[TestCase("dd", "32")] // out of range
	[TestCase("dd d", "31 1")] // mismatched days
	[TestCase("y ddd", "2000 Sa")] // insufficient characters
	[TestCase("y ddd", "2000 Set")] // unknown prefix
	[TestCase("y dddd", "2000 Satyr")] // insufficient characters
	[TestCase("y dddd", "2000 Saturn")] // unknown prefix
	[TestCase("y dddd", "2000 Saturda")] // unknown suffix
	[TestCase("y dddd", "2000 Sunday")] // wrong day of week
	[TestCase("%M", "")] // insufficient digits
	[TestCase("%M", ":")] // invalid digit
	[TestCase("%M", "0")] // out of range
	[TestCase("%M", "13")] // out of range
	[TestCase("MM", "1")] // insufficient digits
	[TestCase("MM", "00")] // out of range
	[TestCase("MM", "13")] // out of range
	[TestCase("MM M", "12 1")] // mismatched months
	[TestCase("MMM", "Ja")] // insufficient characters
	[TestCase("MMM", "Jam")] // unknown prefix
	[TestCase("MMMM", "Ja")] // insufficient characters
	[TestCase("MMMM", "Jam")] // unknown prefix
	[TestCase("MMMM", "Jane")] // unknown suffix
	[TestCase("MMMM MMM", "December Jan")] // mismatched months
	[TestCase("%y", "")] // insufficient digits
	[TestCase("%y", ":")] // invalid digit
	[TestCase("%y", "0")] // out of range
	[TestCase("yy", "0")] // insufficient digits
	[TestCase("yyyy", "001")] // insufficient digits
	[TestCase("yyyy", "0000")] // out of range
	[TestCase("yyyy y", "9999 1")] // mismatched years
	[TestCase("yyyy yy", "9999 01")] // mismatched years
	[TestCase("yyyy-MM-dd", "2000/06/15")] // wrong separator
	[TestCase("yyyy-MM-dd", "2000-06-31")] // day out of range for year and month
	[TestCase("gg", "A.")] // insufficient characters
	public void TryParseExact_WithInvariantCultureAndInvalidString_ReturnsExpected(String? format, String s)
	{
		// Arrange:
		DatePattern pattern = DatePattern.Create(format, CultureInfo.InvariantCulture);

		// Act:
		Boolean actualResult = pattern.TryParseExact(s, out Date actualDate);

		// Assert:
		actualResult.Should().BeFalse();
		actualDate.Should().Be(Date.Empty);
	}

	[TestCase("%d", "1", 1, 1, 1)]
	[TestCase("%d", "01", 1, 1, 1)]
	[TestCase("%d", "31", 1, 1, 31)]
	[TestCase("dd", "01", 1, 1, 1)]
	[TestCase("dd", "31", 1, 1, 31)]
	[TestCase("y ddd", "2000 Sat", 2000, 1, 1)]
	[TestCase("y dddd", "2000 Saturday", 2000, 1, 1)]
	[TestCase("%M", "1", 1, 1, 1)]
	[TestCase("%M", "01", 1, 1, 1)]
	[TestCase("%M", "12", 1, 12, 1)]
	[TestCase("MM", "01", 1, 1, 1)]
	[TestCase("MM", "12", 1, 12, 1)]
	[TestCase("MMM", "Jan", 1, 1, 1)]
	[TestCase("MMM", "Dec", 1, 12, 1)]
	[TestCase("MMMM", "January", 1, 1, 1)]
	[TestCase("MMMM", "December", 1, 12, 1)]
	[TestCase("%y", "1", 1, 1, 1)]
	[TestCase("%y", "0001", 1, 1, 1)]
	[TestCase("%y", "9999", 9999, 1, 1)]
	[TestCase("yyyy", "0001", 1, 1, 1)]
	[TestCase("yyyy", "9999", 9999, 1, 1)]
	[TestCase("M/d/y", "6/15/2000", 2000, 6, 15)]
	[TestCase("yyyy-MM-dd", "2000-06-15", 2000, 6, 15)]
	[TestCase("ddd, dd MMM yyyy", "Thu, 15 Jun 2000", 2000, 6, 15)]
	[TestCase("dddd, dd MMMM yyyy", "Thursday, 15 June 2000", 2000, 6, 15)]
	[TestCase("<''\"\"\\d=\\'d\\\" 'M=\\''M'\"' \"y='\"y\"\\\"\" g>", "<d='15\" M='6\" y='2000\" A.D.>", 2000, 6, 15)]
	[TestCase("yyyy yy y MMMM MMM MM M dddd ddd dd d", "2000 00 2000 June Jun 06 6 Thursday Thu 15 15", 2000, 6, 15)]
	public void TryParseExact_WithInvariantCultureAndValidString_ReturnsExpected(
		String? format, String s, Int32 expectedYear, Int32 expectedMonth, Int32 expectedDay)
	{
		// Arrange:
		DatePattern pattern = DatePattern.Create(format, CultureInfo.InvariantCulture);

		// Act:
		Boolean actualResult = pattern.TryParseExact(s, out Date actualDate);

		// Assert:
		actualResult.Should().BeTrue();
		actualDate.Year.Should().Be(expectedYear);
		actualDate.Month.Should().Be(expectedMonth);
		actualDate.Day.Should().Be(expectedDay);
	}

	[TestCase("6/15/00", 99, null)]
	[TestCase("6/15/99", 99, 99)]
	[TestCase("6/15/00", 2049, 2000)]
	[TestCase("6/15/49", 2049, 2049)]
	[TestCase("6/15/50", 2049, 1950)]
	[TestCase("6/15/99", 2049, 1999)]
	public void TryParseExact_WithTwoDigitYear_ReturnsExpected(String s, Int32 twoDigitYearMax, Int32? expectedYear)
	{
		// Arrange:
		CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
		culture.DateTimeFormat.Calendar.TwoDigitYearMax = twoDigitYearMax;
		DatePattern pattern = DatePattern.Create("M/d/yy", culture);

		// Act:
		Boolean actualResult = pattern.TryParseExact(s, out Date actualDate);

		// Assert:
		if (expectedYear == null)
		{
			actualResult.Should().BeFalse();
			actualDate.Should().Be(Date.Empty);
		}
		else
		{
			actualResult.Should().BeTrue();
			actualDate.Year.Should().Be(expectedYear);
		}
	}

	#endregion
}
