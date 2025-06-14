using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for an event that occurs every week on one or more specified days of the week.
/// </summary>
/// <threadsafety static="true" instance="true" />
public sealed class DaysOfWeekRecurrence : Schedule
{
	private const Int32 LookupSize = 4; // number of bits per entry
	private const Int32 LookupMask = (1 << LookupSize) - 1;

	private static Int32 CreateLookupEntry(DayOfWeek key, DayOfWeek value)
	{
		return (Int32)value << ((Int32)key * LookupSize);
	}

	private static DayOfWeek GetLookupValue(Int32 lookup, DayOfWeek key)
	{
		return (DayOfWeek)((lookup >>> ((Int32)key * LookupSize)) & LookupMask);
	}

	private readonly Int32 _previousDayOfWeekLookup;
	private readonly Int32 _nextDayOfWeekLookup;

	/// <overloads>
	/// Initializes a new instance of the <see cref="DaysOfWeekRecurrence" /> class.
	/// </overloads>
	/// <summary>
	/// Initializes a new instance of the <see cref="DaysOfWeekRecurrence" /> class with the default options.
	/// </summary>
	/// <param name="daysOfWeek">
	/// A bitwise combination of one or more <see cref="Schedules.DaysOfWeek" /> values that specify
	/// the days of the week on which the event occurs.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="daysOfWeek" /> is zero or specifies an invalid day of the week.
	/// </exception>
	public DaysOfWeekRecurrence(DaysOfWeek daysOfWeek)
		: this(daysOfWeek, DaysOfWeekRecurrenceOptions.Default)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DaysOfWeekRecurrence" /> class with specified options.
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
	/// The constructor makes a copy of the <paramref name="options" />, so any subsequent changes to them
	/// will not affect the recurrence.
	/// </remarks>
	public DaysOfWeekRecurrence(DaysOfWeek daysOfWeek, DaysOfWeekRecurrenceOptions options)
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

		Int32 previousDayOfWeekLookup = CreatePreviousDayOfWeekLookup(daysOfWeek);
		Int32 nextDayOfWeekLookup = CreateNextDayOfWeekLookup(daysOfWeek);
		this.DaysOfWeek = daysOfWeek;
		this.StartDate = GetActualStartDate(daysOfWeek, nextDayOfWeekLookup, options.StartDate);
		this.EndDate = GetActualEndDate(daysOfWeek, previousDayOfWeekLookup, options.EndDate);
		_previousDayOfWeekLookup = previousDayOfWeekLookup;
		_nextDayOfWeekLookup = nextDayOfWeekLookup;

		static Int32 CreatePreviousDayOfWeekLookup(DaysOfWeek daysOfWeek)
		{
			Int32 previousDayOfWeekLookup = 0;
			DayOfWeek previousDayOfWeek = (DayOfWeek)BitCounting.GetHighestSetBit((Int32)daysOfWeek);

			for (DayOfWeek dayOfWeek = DayOfWeek.Sunday; dayOfWeek <= DayOfWeek.Saturday; ++dayOfWeek)
			{
				previousDayOfWeekLookup |= CreateLookupEntry(dayOfWeek, previousDayOfWeek);
				if (daysOfWeek.Contains(dayOfWeek))
					previousDayOfWeek = dayOfWeek;
			}

			return previousDayOfWeekLookup;
		}

		static Int32 CreateNextDayOfWeekLookup(DaysOfWeek daysOfWeek)
		{
			Int32 nextDayOfWeekLookup = 0;
			DayOfWeek nextDayOfWeek = (DayOfWeek)BitCounting.GetLowestSetBit((Int32)daysOfWeek);

			for (DayOfWeek dayOfWeek = DayOfWeek.Saturday; dayOfWeek >= DayOfWeek.Sunday; --dayOfWeek)
			{
				nextDayOfWeekLookup |= CreateLookupEntry(dayOfWeek, nextDayOfWeek);
				if (daysOfWeek.Contains(dayOfWeek))
					nextDayOfWeek = dayOfWeek;
			}

			return nextDayOfWeekLookup;
		}

		static Date GetActualStartDate(DaysOfWeek daysOfWeek, Int32 nextDayOfWeekLookup, Date startDate)
		{
			DayOfWeek dayOfWeek = startDate.DayOfWeek;
			return daysOfWeek.Contains(dayOfWeek)
				? startDate
				: GetNextOccurrence(nextDayOfWeekLookup, startDate, dayOfWeek, out _);
		}

		static Date GetActualEndDate(DaysOfWeek daysOfWeek, Int32 previousDayOfWeekLookup, Date endDate)
		{
			DayOfWeek dayOfWeek = endDate.DayOfWeek;
			return daysOfWeek.Contains(dayOfWeek)
				? endDate
				: GetPreviousOccurrence(previousDayOfWeekLookup, endDate, dayOfWeek, out _);
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
		return date >= this.StartDate && date <= this.EndDate && this.DaysOfWeek.Contains(date.DayOfWeek);
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

			Int32 previousDayOfWeekLookup = _previousDayOfWeekLookup;

			while (date > startDate)
			{
				date = GetPreviousOccurrence(previousDayOfWeekLookup, date, dayOfWeek, out dayOfWeek);
				yield return date;
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

			Int32 nextDayOfWeekLookup = _nextDayOfWeekLookup;

			while (date < endDate)
			{
				date = GetNextOccurrence(nextDayOfWeekLookup, date, dayOfWeek, out dayOfWeek);
				yield return date;
			}
		}
	}

	private static Date GetPreviousOccurrence(
		Int32 previousDayOfWeekLookup, Date date, DayOfWeek dayOfWeek, out DayOfWeek previousDayOfWeek)
	{
		Debug.Assert(date.DayOfWeek == dayOfWeek);

		previousDayOfWeek = GetLookupValue(previousDayOfWeekLookup, dayOfWeek);
		Int32 dayOffset = previousDayOfWeek - dayOfWeek;
		if (dayOffset >= 0)
			dayOffset -= Date.DaysPerWeek;
		return Date.AddSmallNegativeDays(date, dayOffset);
	}

	private static Date GetNextOccurrence(
		Int32 nextDayOfWeekLookup, Date date, DayOfWeek dayOfWeek, out DayOfWeek nextDayOfWeek)
	{
		Debug.Assert(date.DayOfWeek == dayOfWeek);

		nextDayOfWeek = GetLookupValue(nextDayOfWeekLookup, dayOfWeek);
		Int32 dayOffset = nextDayOfWeek - dayOfWeek;
		if (dayOffset <= 0)
			dayOffset += Date.DaysPerWeek;
		return Date.AddSmallPositiveDays(date, dayOffset);
	}
}
