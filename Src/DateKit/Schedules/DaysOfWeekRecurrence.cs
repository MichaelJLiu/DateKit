using System;
using System.Collections.Generic;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for an event that occurs every week on one or more specified days of the week.
/// </summary>
/// <threadsafety static="true" instance="true" />
public sealed class DaysOfWeekRecurrence : Schedule
{
	private sealed class DayOfWeekEntry
	{
		public DayOfWeekEntry PreviousEntry = null!;
		public DayOfWeekEntry NextEntry = null!;
		public Int32 PreviousDayOffset;
		public Int32 NextDayOffset;
	}

	/// <overloads>
	/// Creates a <see cref="DaysOfWeekRecurrence" />.
	/// </overloads>
	/// <summary>
	/// Creates a <see cref="DaysOfWeekRecurrence" /> using specified <see cref="Schedules.DaysOfWeek" />
	/// and the default <see cref="DaysOfWeekRecurrenceOptions" />.
	/// </summary>
	/// <param name="daysOfWeek">
	/// A bitwise combination of one or more <see cref="Schedules.DaysOfWeek" /> values that specify
	/// the days of the week on which the event occurs.
	/// </param>
	/// <returns>
	/// A <see cref="DaysOfWeekRecurrence" /> that occurs on <paramref name="daysOfWeek" />,
	/// using the default <see cref="DaysOfWeekRecurrenceOptions" />.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="daysOfWeek" /> is zero or specifies an invalid day of the week.
	/// </exception>
	public static DaysOfWeekRecurrence Create(DaysOfWeek daysOfWeek)
	{
		return Create(daysOfWeek, DaysOfWeekRecurrenceOptions.Default);
	}

	/// <summary>
	/// Creates a <see cref="DaysOfWeekRecurrence" /> using specified <see cref="Schedules.DaysOfWeek" />
	/// and <see cref="DaysOfWeekRecurrenceOptions" />.
	/// </summary>
	/// <param name="daysOfWeek">
	/// A bitwise combination of one or more <see cref="Schedules.DaysOfWeek" /> values that specify
	/// the days of the week on which the event occurs.
	/// </param>
	/// <param name="options">
	/// A <see cref="DaysOfWeekRecurrenceOptions" /> instance that specifies options for the recurrence.
	/// </param>
	/// <returns>
	/// A <see cref="DaysOfWeekRecurrence" /> that occurs on <paramref name="daysOfWeek" />,
	/// using <paramref name="options" />.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="options" /> is <see langword="null" />.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="daysOfWeek" /> is zero or specifies an invalid day of the week.
	/// </exception>
	/// <remarks>
	/// The method makes a copy of <paramref name="options" />, so any subsequent changes to them will not affect
	/// the recurrence.
	/// </remarks>
	public static DaysOfWeekRecurrence Create(DaysOfWeek daysOfWeek, DaysOfWeekRecurrenceOptions options)
	{
		return new DaysOfWeekRecurrence(daysOfWeek, options);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DaysOfWeekRecurrence" /> class.
	/// </summary>
	/// <param name="daysOfWeek">
	/// A bitwise combination of one or more <see cref="Schedules.DaysOfWeek" /> values that specify
	/// the days of the week on which the event occurs.
	/// </param>
	/// <param name="options">
	/// A <see cref="DaysOfWeekRecurrenceOptions" /> instance that specifies options for the recurrence.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="options" /> is <see langword="null" />.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="daysOfWeek" /> is zero or specifies an invalid day of the week.
	/// </exception>
	/// <remarks>
	/// The constructor makes a copy of <paramref name="options" />, so any subsequent changes to them will not affect
	/// the recurrence.
	/// </remarks>
	private DaysOfWeekRecurrence(DaysOfWeek daysOfWeek, DaysOfWeekRecurrenceOptions options)
	{
		ThrowHelper.ThrowIfArgumentIsNull(options);

		if (daysOfWeek == 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(daysOfWeek), daysOfWeek, "The value must specify one or more days of the week.");
		}

		const DaysOfWeek allDaysOfWeek = (DaysOfWeek)((1 << Date.DaysPerWeek) - 1);
		if ((daysOfWeek & ~allDaysOfWeek) != 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(daysOfWeek), daysOfWeek, "The value specifies an invalid day of the week.");
		}

		this.DaysOfWeek = daysOfWeek;
		DayOfWeekEntry[] entries = this.DayOfWeekEntries = CreateDayOfWeekEntries(daysOfWeek);
		this.StartDate = GetActualStartDate(daysOfWeek, entries, options.StartDate);
		this.EndDate = GetActualEndDate(daysOfWeek, entries, options.EndDate);

		static DayOfWeekEntry[] CreateDayOfWeekEntries(DaysOfWeek daysOfWeek)
		{
			DayOfWeekEntry[] entries = new DayOfWeekEntry[Date.DaysPerWeek];
			for (Int32 dayOfWeek = 0; dayOfWeek < Date.DaysPerWeek; ++dayOfWeek)
				entries[dayOfWeek] = new DayOfWeekEntry();

			// Initialize the PreviousDayOffset and PreviousEntry fields of each entry:
			Int32 previousDayOfWeek = BitCounting.GetHighestSetBit((Int32)daysOfWeek);
			DayOfWeekEntry previousEntry = entries[previousDayOfWeek];
			Int32 previousDayOffset = previousDayOfWeek - (Date.DaysPerWeek - 1);

			for (Int32 dayOfWeek = 0; dayOfWeek < Date.DaysPerWeek; ++dayOfWeek)
			{
				DayOfWeekEntry entry = entries[dayOfWeek];
				entry.PreviousEntry = previousEntry;
				entry.PreviousDayOffset = --previousDayOffset;

				if (daysOfWeek.Contains((DayOfWeek)dayOfWeek))
				{
					previousEntry = entry;
					previousDayOffset = 0;
				}
			}

			// Initialize the NextDayOffset and NextEntry fields of each entry:
			Int32 nextDayOfWeek = BitCounting.GetLowestSetBit((Int32)daysOfWeek);
			DayOfWeekEntry nextEntry = entries[nextDayOfWeek];
			Int32 nextDayOffset = nextDayOfWeek;

			for (Int32 dayOfWeek = Date.DaysPerWeek - 1; dayOfWeek >= 0; --dayOfWeek)
			{
				DayOfWeekEntry entry = entries[dayOfWeek];
				entry.NextEntry = nextEntry;
				entry.NextDayOffset = ++nextDayOffset;

				if (daysOfWeek.Contains((DayOfWeek)dayOfWeek))
				{
					nextEntry = entry;
					nextDayOffset = 0;
				}
			}

			return entries;
		}

		static Date GetActualStartDate(DaysOfWeek daysOfWeek, DayOfWeekEntry[] entries, Date startDate)
		{
			DayOfWeek dayOfWeek = startDate.DayOfWeek;
			return daysOfWeek.Contains(dayOfWeek)
				? startDate
				: Date.AddSmallPositiveDays(startDate, entries[(Int32)dayOfWeek].NextDayOffset);
		}

		static Date GetActualEndDate(DaysOfWeek daysOfWeek, DayOfWeekEntry[] entries, Date endDate)
		{
			DayOfWeek dayOfWeek = endDate.DayOfWeek;
			return daysOfWeek.Contains(dayOfWeek)
				? endDate
				: Date.AddSmallNegativeDays(endDate, entries[(Int32)dayOfWeek].PreviousDayOffset);
		}
	}

	/// <summary>
	/// Gets the days of the week on which the event occurs.
	/// </summary>
	/// <value>
	/// A bitwise combination of one or more <see cref="Schedules.DaysOfWeek" /> values that specify
	/// the days of the week on which the event occurs.
	/// </value>
	public DaysOfWeek DaysOfWeek { get; }

	private DayOfWeekEntry[] DayOfWeekEntries { get; }

	/// <summary>
	/// Gets the date on which the recurrence starts.
	/// </summary>
	/// <value>
	/// The <see cref="Date" /> on which the recurrence starts.
	/// </value>
	public Date StartDate { get; }

	/// <summary>
	/// Gets the date on which the recurrence ends.
	/// </summary>
	/// <value>
	/// The <see cref="Date" /> on which the recurrence ends.
	/// </value>
	public Date EndDate { get; }

	/// <inheritdoc />
	protected internal override Boolean ContainsCore(Date date)
	{
		return this.DaysOfWeek.Contains(date.DayOfWeek) && date >= this.StartDate && date <= this.EndDate;
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateBackwardFromCore(Date date)
	{
		Date startDate = this.StartDate;

		if (date >= startDate)
		{
			Date endDate = this.EndDate;
			DayOfWeek dayOfWeek;

			if (date < endDate)
			{
				dayOfWeek = date.DayOfWeek;
				if (this.DaysOfWeek.Contains(dayOfWeek))
					yield return date;
			}
			else
			{
				date = endDate;
				dayOfWeek = date.DayOfWeek;
				yield return date;
			}

			DayOfWeekEntry entry = this.DayOfWeekEntries[(Int32)dayOfWeek];

			while (date > startDate)
			{
				date = Date.AddSmallNegativeDays(date, entry.PreviousDayOffset);
				yield return date;
				entry = entry.PreviousEntry;
			}
		}
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateForwardFromCore(Date date)
	{
		Date endDate = this.EndDate;

		if (date <= endDate)
		{
			Date startDate = this.StartDate;
			DayOfWeek dayOfWeek;

			if (date > startDate)
			{
				dayOfWeek = date.DayOfWeek;
				if (this.DaysOfWeek.Contains(dayOfWeek))
					yield return date;
			}
			else
			{
				date = startDate;
				dayOfWeek = date.DayOfWeek;
				yield return date;
			}

			DayOfWeekEntry entry = this.DayOfWeekEntries[(Int32)dayOfWeek];

			while (date < endDate)
			{
				date = Date.AddSmallPositiveDays(date, entry.NextDayOffset);
				yield return date;
				entry = entry.NextEntry;
			}
		}
	}
}
