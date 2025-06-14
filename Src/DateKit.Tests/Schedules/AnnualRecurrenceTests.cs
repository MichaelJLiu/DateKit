using System;

using NUnit.Framework;

namespace DateKit.Schedules;

[TestFixture]
public class AnnualRecurrenceTests
{
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
