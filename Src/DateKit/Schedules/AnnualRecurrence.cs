using System;
using System.Collections.Generic;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for an event that occurs once every year.
/// </summary>
/// <threadsafety static="true" instance="true" />
public abstract class AnnualRecurrence : Schedule
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AnnualRecurrence" /> class.
	/// </summary>
	/// <param name="options">
	/// An <see cref="AnnualRecurrenceOptions" /> instance that specifies options for the recurrence.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="options" /> is <see langword="null" />.
	/// </exception>
	/// <remarks>
	/// The constructor makes a copy of <paramref name="options" />, so any subsequent changes to them will not affect
	/// the recurrence.
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
	/// Gets the date on which the event occurs in a specified year.
	/// </summary>
	/// <param name="year">
	/// An integer between <see cref="StartYear" /> and <see cref="EndYear" /> that specifies a year.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> on which the event occurs in <paramref name="year" />,
	/// or <see cref="Date.Empty" /> if the event does not occur in <paramref name="year" />.
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

	/// <inheritdoc />
	protected internal override Boolean ContainsCore(Date date)
	{
		return this.ContainsCore(date, date.Year);
	}

	private protected Boolean ContainsCore(Date date, Int32 year)
	{
		return year >= this.StartYear && year <= this.EndYear && date == this.GetOccurrenceCore(year);
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateBackwardFromCore(Date date)
	{
		return this.EnumerateBackwardFromCore(date, date.Year);
	}

	private protected IEnumerable<Date> EnumerateBackwardFromCore(Date date, Int32 year)
	{
		Int32 startYear = this.StartYear;

		if (year >= startYear)
		{
			Int32 endYear = this.EndYear;

			if (year <= endYear)
			{
				Date firstOccurrence = this.GetOccurrenceCore(year);
				if (firstOccurrence <= date)
					yield return firstOccurrence;
			}
			else
			{
				year = endYear;
				yield return this.GetOccurrenceCore(year);
			}

			while (year > startYear)
			{
				--year;
				yield return this.GetOccurrenceCore(year);
			}
		}
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateForwardFromCore(Date date)
	{
		return this.EnumerateForwardFromCore(date, date.Year);
	}

	private protected IEnumerable<Date> EnumerateForwardFromCore(Date date, Int32 year)
	{
		Int32 endYear = this.EndYear;

		if (year <= endYear)
		{
			Int32 startYear = this.StartYear;

			if (year >= startYear)
			{
				Date firstOccurrence = this.GetOccurrenceCore(year);
				if (firstOccurrence >= date)
					yield return firstOccurrence;
			}
			else
			{
				year = startYear;
				yield return this.GetOccurrenceCore(year);
			}

			while (year < endYear)
			{
				++year;
				yield return this.GetOccurrenceCore(year);
			}
		}
	}

	/// <summary>
	/// Gets the date on which the event occurs in a specified year.
	/// </summary>
	/// <param name="year">
	/// An integer between <see cref="StartYear" /> and <see cref="EndYear" /> that specifies a year.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> on which the event occurs in <paramref name="year" />,
	/// or <see cref="Date.Empty" /> if the event does not occur in <paramref name="year" />.
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
