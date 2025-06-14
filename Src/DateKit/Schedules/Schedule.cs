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
	/// <see langword="true" /> if the schedule contains <paramref name="date" />; otherwise, <see langword="false" />.
	/// </returns>
	/// <remarks>
	/// If <paramref name="date" /> is <see cref="Date.Empty" />, the method returns <see langword="false" />.
	/// </remarks>
#pragma warning disable CA1716 // Identifiers should not match keywords (date)
	public Boolean Contains(Date date)
	{
		return date != Date.Empty && this.ContainsCore(date);
	}
#pragma warning restore CA1716

	/// <summary>
	/// Enumerates dates in the schedule starting from a specified date and continuing backward in time.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> from which to start the enumeration.
	/// </param>
	/// <returns>
	/// A <see cref="Date" /> sequence that contains, in reverse chronological order, all the dates in the schedule
	/// that are equal to or earlier than <paramref name="date" />.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="date" /> is <see cref="Date.Empty" />.
	/// </exception>
#pragma warning disable CA1716 // Identifiers should not match keywords (date)
	public IEnumerable<Date> EnumerateBackwardFrom(Date date)
	{
		ThrowHelper.ThrowIfDateArgumentIsEmpty(date, ExceptionArgument.date);
		return this.EnumerateBackwardFromCore(date);
	}
#pragma warning restore CA1716

	/// <summary>
	/// Enumerates dates in the schedule starting from a specified date and continuing forward in time.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> from which to start the enumeration.
	/// </param>
	/// <returns>
	/// A <see cref="Date" /> sequence that contains, in chronological order, all the dates in the schedule
	/// that are equal to or later than <paramref name="date" />.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="date" /> is <see cref="Date.Empty" />.
	/// </exception>
#pragma warning disable CA1716 // Identifiers should not match keywords (date)
	public IEnumerable<Date> EnumerateForwardFrom(Date date)
	{
		ThrowHelper.ThrowIfDateArgumentIsEmpty(date, ExceptionArgument.date);
		return this.EnumerateForwardFromCore(date);
	}
#pragma warning restore CA1716

	/// <summary>
	/// Determines whether the schedule contains a specified date.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> to find in the schedule.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if the schedule contains <paramref name="date" />; otherwise, <see langword="false" />.
	/// </returns>
	/// <remarks>
	/// <note type="implement">
	/// This method provides the core implementation of the <see cref="Contains" /> method, which invokes
	/// this method after validating that <paramref name="date" /> is not <see cref="Date.Empty" />.
	/// </note>
	/// </remarks>
#pragma warning disable CA1716 // Identifiers should not match keywords (date)
	protected internal abstract Boolean ContainsCore(Date date);
#pragma warning restore CA1716

	/// <summary>
	/// Enumerates dates in the schedule starting from a specified date and continuing backward in time.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> from which to start the enumeration.
	/// </param>
	/// <returns>
	/// A <see cref="Date" /> sequence that contains, in reverse chronological order, all the dates in the schedule
	/// that are equal to or earlier than <paramref name="date" />.
	/// </returns>
	/// <remarks>
	/// <note type="implement">
	/// This method provides the core implementation of the <see cref="EnumerateBackwardFrom" /> method, which invokes
	/// this method after validating that <paramref name="date" /> is not <see cref="Date.Empty" />.
	/// </note>
	/// </remarks>
#pragma warning disable CA1716 // Identifiers should not match keywords (date)
	protected internal abstract IEnumerable<Date> EnumerateBackwardFromCore(Date date);
#pragma warning restore CA1716

	/// <summary>
	/// Enumerates dates in the schedule starting from a specified date and continuing forward in time.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> from which to start the enumeration.
	/// </param>
	/// <returns>
	/// A <see cref="Date" /> sequence that contains, in chronological order, all the dates in the schedule
	/// that are equal to or later than <paramref name="date" />.
	/// </returns>
	/// <remarks>
	/// <note type="implement">
	/// This method provides the core implementation of the <see cref="EnumerateForwardFrom" /> method, which invokes
	/// this method after validating that <paramref name="date" /> is not <see cref="Date.Empty" />.
	/// </note>
	/// </remarks>
#pragma warning disable CA1716 // Identifiers should not match keywords (date)
	protected internal abstract IEnumerable<Date> EnumerateForwardFromCore(Date date);
#pragma warning restore CA1716
}
