using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

/// <summary>
/// Tests the <see cref="GregorianEasterRecurrence" /> class.
/// </summary>
[TestFixture]
public class GregorianEasterRecurrenceTests
{
	#region Create

	[Test]
	public void Create_WithNullOptions_ThrowsException()
	{
		// Arrange:
		GregorianEasterRecurrenceOptions options = null!;

		// Act:
		Func<GregorianEasterRecurrence> func = () => GregorianEasterRecurrence.Create(options);

		// Assert:
		func.Should().Throw<ArgumentNullException>().WithParameterName("options");
	}

	[Test]
	public void Create_WithDefaultOptions_ReturnsInstance()
	{
		// Act:
		GregorianEasterRecurrence recurrence = GregorianEasterRecurrence.Create();

		// Assert:
		recurrence.StartYear.Should().Be(Date.MinYear);
		recurrence.EndYear.Should().Be(Date.MaxYear);
		recurrence.Offset.Should().Be(0);
	}

	#endregion

	#region Contains

	[TestCaseSource(nameof(GetContainsTestCases))]
	public void Contains_WithValidDate_ReturnsExpected(Date date, Boolean expectedResult)
	{
		// Arrange:
		GregorianEasterRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = GregorianEasterRecurrence.Create(options);

		// Act:
		Boolean actualResult = schedule.Contains(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	private static IEnumerable<Object[]> GetContainsTestCases()
	{
		return
		[
			CreateTestCase(new Date(1999, 4, 4), false), // before StartYear
			CreateTestCase(new Date(2000, 4, 21), false), // wrong day
			CreateTestCase(new Date(2000, 4, 23), true),
			CreateTestCase(new Date(2003, 4, 20), false), // after EndYear
		];

		static Object[] CreateTestCase(Date date, Boolean expectedResult)
		{
			return [date, expectedResult];
		}
	}

	#endregion

	#region EnumerateBackwardFrom

	[TestCaseSource(nameof(GetEnumerateBackwardFromTestCases))]
	public void EnumerateBackwardFrom_WithValidDate_ReturnsExpected(Date date, Date[] expectedResults)
	{
		// Arrange:
		GregorianEasterRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = GregorianEasterRecurrence.Create(options);

		// Act:
		Date[] actualResults = schedule.EnumerateBackwardFrom(date).Take(4).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateBackwardFromTestCases()
	{
		return
		[
			CreateTestCase(new Date(1999, 12, 31), []), // start before StartYear
			CreateTestCase(new Date(2000, 4, 22), []),
			CreateTestCase(new Date(2000, 4, 23), [2000]),
			CreateTestCase(new Date(2002, 12, 31), [2002, 2001, 2000]),
			CreateTestCase(new Date(2003, 12, 31), [2002, 2001, 2000]), // start after EndYear
		];

		static Object[] CreateTestCase(Date date, Int32[] expectedYears)
		{
			Date[] expectedResults = Array.ConvertAll(expectedYears, CalculateEaster);
			return [date, expectedResults];
		}
	}

	#endregion

	#region EnumerateForwardFrom

	[TestCaseSource(nameof(GetEnumerateForwardFromTestCases))]
	public void EnumerateForwardFrom_WithValidDate_ReturnsExpected(Date date, Date[] expectedResults)
	{
		// Arrange:
		GregorianEasterRecurrenceOptions options = new() { StartYear = 2000, EndYear = 2002 };
		Schedule schedule = GregorianEasterRecurrence.Create(options);

		// Act:
		Date[] actualResults = schedule.EnumerateForwardFrom(date).Take(4).ToArray();

		// Assert:
		actualResults.Should().Equal(expectedResults);
	}

	private static IEnumerable<Object[]> GetEnumerateForwardFromTestCases()
	{
		return
		[
			CreateTestCase(new Date(1999, 1, 1), [2000, 2001, 2002]), // start before StartYear
			CreateTestCase(new Date(2000, 1, 1), [2000, 2001, 2002]),
			CreateTestCase(new Date(2002, 3, 31), [2002]),
			CreateTestCase(new Date(2002, 4, 1), []),
			CreateTestCase(new Date(2003, 1, 1), []), // start after EndYear
		];

		static Object[] CreateTestCase(Date date, Int32[] expectedYears)
		{
			Date[] expectedResults = Array.ConvertAll(expectedYears, CalculateEaster);
			return [date, expectedResults];
		}
	}

	#endregion

	#region GetOccurrence

	[Test]
	public void GetOccurrence_WithScheduledYear_ReturnsExpected()
	{
		// Arrange:
		AnnualRecurrence recurrence = GregorianEasterRecurrence.Create();

		for (Int32 year = Date.MinYear; year <= Date.MaxYear; ++year)
		{
			// Act:
			Date actualResult = recurrence.GetOccurrence(year);

			// Assert:
			Date expectedResult = CalculateEaster(year);
			actualResult.Should().Be(expectedResult);
		}
	}

	private static Date CalculateEaster(Int32 year)
	{
		// New Scientist algorithm, https://en.wikipedia.org/wiki/Date_of_Easter#Anonymous_Gregorian_algorithm
		Int32 a = year % 19;
		Int32 b = year / 100;
		Int32 c = year % 100;
		Int32 d = b / 4;
		Int32 e = b % 4;
		Int32 g = (8 * b + 13) / 25;
		Int32 h = (19 * a + b - d - g + 15) % 30;
		Int32 i = c / 4;
		Int32 k = c % 4;
		Int32 l = (32 + 2 * e + 2 * i - h - k) % 7;
		Int32 m = (a + 11 * h + 19 * l) / 433;
		Int32 month = (h + l - 7 * m + 90) / 25;
		Int32 day = (h + l - 7 * m + 33 * month + 19) % 32;
		return new Date(year, month, day);
	}

	#endregion
}
