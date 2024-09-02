using System;
using System.Collections.Generic;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for a one-time or recurring event.
/// </summary>
/// <threadsafety static="true" instance="true" />
public abstract class Schedule
{
	/// <summary>
	/// Determines whether the schedule contains a specified date.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> to find in the schedule.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if the schedule contains the specified <paramref name="date" />;
	/// otherwise, <see langword="false" />.
	/// </returns>
	public abstract Boolean Contains(Date date);

	/// <summary>
	/// Enumerates dates in the schedule starting from a specified date and continuing backward in time.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> from which to start the enumeration.
	/// </param>
	/// <returns>
	/// A <see cref="Date" /> sequence that contains, in reverse chronological order, all the dates in the schedule
	/// that are equal to or earlier than the specified <paramref name="date" />.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="date" /> is <see cref="Date.Empty" />.
	/// </exception>
	public abstract IEnumerable<Date> EnumerateBackwardFrom(Date date);

	/// <summary>
	/// Enumerates dates in the schedule starting from a specified date and continuing forward in time.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> from which to start the enumeration.
	/// </param>
	/// <returns>
	/// A <see cref="Date" /> sequence that contains, in chronological order, all the dates in the schedule
	/// that are equal to or later than the specified <paramref name="date" />.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="date" /> is <see cref="Date.Empty" />.
	/// </exception>
	public abstract IEnumerable<Date> EnumerateForwardFrom(Date date);
}
