using System;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for an event that occurs once every year.
/// </summary>
/// <threadsafety static="true" instance="true" />
public abstract class AnnualRecurrence : Schedule
{
	/// <summary>
	/// Gets the date on which the event occurs in a specified year.
	/// </summary>
	/// <param name="year">
	/// An integer between <see cref="Date.MinYear" /> and <see cref="Date.MaxYear" /> that specifies a year.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> on which the event occurs in the specified <paramref name="year" />,
	/// or <see cref="Date.Empty" /> if the event does not occur in the specified <paramref name="year" />.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="year" /> is less than <see cref="Date.MinYear" /> or greater than <see cref="Date.MaxYear" />.
	/// </exception>
	public abstract Date GetOccurrence(Int32 year);
}
