using System;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for an event that occurs once every year.
/// </summary>
/// <threadsafety static="true" instance="true" />
public abstract class AnnualRecurrence : Schedule
{
	/// <summary>
	/// Gets the first year in which the event occurs.
	/// </summary>
	/// <value>
	/// An integer between <see cref="Date.MinYear" /> and <see cref="EndYear" /> that specifies the first year
	/// in which the event occurs.
	/// </value>
	public abstract Int32 StartYear { get; }

	/// <summary>
	/// Gets the last year in which the event occurs.
	/// </summary>
	/// <value>
	/// An integer between <see cref="StartYear" /> and <see cref="Date.MaxYear" /> that specifies the last year
	/// in which the event occurs.
	/// </value>
	public abstract Int32 EndYear { get; }

	/// <summary>
	/// Gets the date on which the event occurs in a specified year.
	/// </summary>
	/// <param name="year">
	/// An integer between <see cref="StartYear" /> and <see cref="EndYear" /> that specifies a year.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> on which the event occurs in the specified <paramref name="year" />,
	/// or <see cref="Date.Empty" /> if the event does not occur in the specified <paramref name="year" />.
	/// </returns>
	/// <remarks>
	/// If <paramref name="year" /> is less than <see cref="StartYear" /> or greater than <see cref="EndYear" />,
	/// the method returns <see cref="Date.Empty" />.
	/// </remarks>
	public abstract Date GetOccurrence(Int32 year);
}
