using System;

using FluentAssertions;

using NUnit.Framework;

namespace DateKit.Schedules;

[TestFixture]
public class AnnualRecurrenceTests
{
	#region Contains

	// ReSharper disable NUnit.IncorrectArgumentType
	[TestCase(1997, "", false)] // before StartYear
	[TestCase(1998, "1998-06-15", true)]
	[TestCase(2000, "2000-06-30", false)] // wrong date
	[TestCase(2002, "2002-06-15", true)]
	[TestCase(2003, "", false)] // after EndYear
	// ReSharper restore NUnit.IncorrectArgumentType
	public void Contains_ReturnsExpected(Int32 year, Date occurrence, Boolean expectedResult)
	{
		// Arrange:
		AnnualRecurrenceOptions options = new() { StartYear = 1998, EndYear = 2002 };
		AnnualRecurrence recurrence = new MockAnnualRecurrence(
			options,
			getOccurrence: occurrence != Date.Empty ? [(year, occurrence)] : []);
		Date date = new(year, 6, 15);

		// Act:
		Boolean actualResult = recurrence.Contains(date);

		// Assert:
		actualResult.Should().Be(expectedResult);
	}

	#endregion

	#region GetOccurrence

	[TestCase(Date.MinYear - 1)]
	[TestCase(1997)] // before StartYear
	[TestCase(2003)] // after EndYear
	[TestCase(Date.MaxYear + 1)]
	public void GetOccurrence_WithUnscheduledYear_ReturnsEmptyDate(Int32 year)
	{
		// Arrange:
		AnnualRecurrenceOptions options = new() { StartYear = 1998, EndYear = 2002 };
		AnnualRecurrence recurrence = new MockAnnualRecurrence(options);

		// Act:
		Date actualResult = recurrence.GetOccurrence(year);

		// Assert:
		actualResult.Should().Be(Date.Empty);
	}

	#endregion
}
