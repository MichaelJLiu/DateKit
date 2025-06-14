using System;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for an event that occurs once every year.
/// </summary>
/// <threadsafety static="true" instance="true" />
public abstract class AnnualRecurrence : Schedule
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AnnualRecurrence" /> class with specified options.
	/// </summary>
	/// <param name="options">
	/// An <see cref="AnnualRecurrenceOptions" /> instance that specifies options for the recurrence.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="options" /> is <see langword="null" />.
	/// </exception>
	/// <remarks>
	/// The constructor makes a copy of the <paramref name="options" />, so any subsequent changes to them
	/// will not affect the recurrence.
	/// </remarks>
	protected AnnualRecurrence(AnnualRecurrenceOptions options)
	{
		ThrowHelper.ThrowIfArgumentIsNull(options);

		this.StartYear = options.StartYear;
		this.EndYear = options.EndYear;
	}

	/// <summary>
	/// Gets the year in which the recurrence starts.
	/// </summary>
	/// <value>
	/// An integer between <see cref="Date.MinYear" /> and <see cref="EndYear" /> that specifies
	/// the year in which the recurrence starts.
	/// </value>
	public Int32 StartYear { get; }

	/// <summary>
	/// Gets the year in which the recurrence ends.
	/// </summary>
	/// <value>
	/// An integer between <see cref="StartYear" /> and <see cref="Date.MaxYear" /> that specifies
	/// the year in which the recurrence ends.
	/// </value>
	public Int32 EndYear { get; }

	/// <summary>
	/// Gets the earliest and latest days of the year on which the event may occur.
	/// </summary>
	/// <param name="minDayOfYear">
	/// When this method returns, contains an integer that specifies the earliest day of the year on which the event
	/// may occur. The value is normally between 1 and 365 (representing January 1 and December 31, respectively)
	/// but may be less than 1 if the event for some year actually occurs in the preceding year.
	/// </param>
	/// <param name="maxDayOfYear">
	/// When this method returns, contains an integer that specifies the latest day of the year on which the event
	/// may occur. The value is normally between 1 and 365 (representing January 1 and December 31, respectively)
	/// but may be greater than 365 if the event for some year actually occurs in the following year.
	/// </param>
	/// <remarks>
	/// <para>
	/// February 29 is treated as March 1 instead of a separate day of the year.
	/// </para>
	/// <para>
	/// An implementation is permitted to return an estimate for <paramref name="minDayOfYear" /> that is less than
	/// the actual value and an estimate for <paramref name="maxDayOfYear" /> that is greater than the actual value.
	/// </para>
	/// </remarks>
	public abstract void GetDayOfYearRange(out Int32 minDayOfYear, out Int32 maxDayOfYear);

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
	public Date GetOccurrence(Int32 year)
	{
		return year >= this.StartYear && year <= this.EndYear
			? this.GetOccurrenceCore(year)
			: Date.Empty;
	}

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
	/// <note type="implement">
	/// This method provides the core implementation of the <see cref="GetOccurrence" /> method, which invokes
	/// this method after validating that <paramref name="year" /> is between <see cref="StartYear" /> and
	/// <see cref="EndYear" />.
	/// </note>
	/// </remarks>
	protected internal abstract Date GetOccurrenceCore(Int32 year);
}
