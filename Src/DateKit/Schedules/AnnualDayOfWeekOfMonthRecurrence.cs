using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for an event that occurs once every year on a specified occurrence of a specified
/// day of the week in a specified month.
/// </summary>
/// <threadsafety static="true" instance="true" />
public class AnnualDayOfWeekOfMonthRecurrence : AnnualRecurrence
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AnnualDayOfWeekOfMonthRecurrence" /> class.
	/// </summary>
	/// <param name="month">
	/// An integer between 1 and 12 that specifies the month of the event.
	/// </param>
	/// <param name="dayOfWeek">
	/// The <see cref="System.DayOfWeek" /> that specifies the day of the week of the event.
	/// </param>
	/// <param name="occurrence">
	/// A nonzero integer between -4 and 4 that specifies the first, second, third, or fourth occurrence
	/// of the <paramref name="dayOfWeek" /> in the <paramref name="month" />. A positive integer counts
	/// from the beginning of the month, whereas a negative integer counts from the end of the month.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <para>
	/// <paramref name="month" /> is less than 1 or greater than 12.
	/// </para>
	/// <para>-or-</para>
	/// <para>
	/// <paramref name="dayOfWeek" /> is less than <see cref="DayOfWeek.Sunday" /> or greater than
	/// <see cref="DayOfWeek.Saturday" />.
	/// </para>
	/// <para>-or-</para>
	/// <para>
	/// <paramref name="occurrence" /> is less than -4, equal to 0, or greater than 4.
	/// </para>
	/// </exception>
	/// <example>
	/// The expression <c>new AnnualDayOfWeekOfMonthRecurrence(5, DayOfWeek.Monday, -1)</c> represents the schedule
	/// for an event that occurs once every year on the last Monday of May.
	/// </example>
	public AnnualDayOfWeekOfMonthRecurrence(Int32 month, DayOfWeek dayOfWeek, Int32 occurrence)
	{
		ThrowHelper.ThrowIfMonthArgumentIsOutOfRange(month, ExceptionArgument.month);
		// Unoptimized:
		//   if (dayOfWeek < DayOfWeek.Sunday || dayOfWeek > DayOfWeek.Saturday)
		// Optimized:
		if (unchecked((UInt32)(dayOfWeek - DayOfWeek.Sunday)) > DayOfWeek.Saturday - DayOfWeek.Sunday)
			throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null);
		// Unoptimized:
		//   if (occurrence == 0 || occurrence < -4 || occurrence > 4)
		// Optimized:
		if (occurrence == 0 || unchecked((UInt32)(occurrence + 4)) > 8)
			throw new ArgumentOutOfRangeException(nameof(occurrence), occurrence, null);

		this.Month = month;
		this.DayOfWeek = dayOfWeek;
		this.Occurrence = occurrence;
		this.MaxDay = (occurrence + (occurrence > 0 ? 0 : 1)) * Date.DaysPerWeek;
	}

	/// <summary>
	/// Gets the month of the event.
	/// </summary>
	/// <value>
	/// An integer between 1 and 12 that specifies the month of the event.
	/// </value>
	public Int32 Month { get; }

	/// <summary>
	/// Gets the day of the week of the event.
	/// </summary>
	/// <value>
	/// The <see cref="System.DayOfWeek" /> that specifies the day of the week of the event.
	/// </value>
	public DayOfWeek DayOfWeek { get; }

	/// <summary>
	/// Gets an integer that specifies the occurrence of the day of the week of the event.
	/// </summary>
	/// <value>
	/// A nonzero integer between -4 and 4 that specifies the first, second, third, or fourth occurrence
	/// of the <see cref="DayOfWeek" /> in the <see cref="Month" />. A positive integer counts
	/// from the beginning of the month, whereas a negative integer counts from the end of the month.
	/// </value>
	public Int32 Occurrence { get; }

	// The latest possible day of the month on which the event can occur.
	// For Occurrence > 0, this is the actual day: 7, 14, 21, or 28.
	// For Occurrence < 0, this is an offset from the last day of the month: 0, -7, -14, or -21.
	private Int32 MaxDay { get; }

	/// <inheritdoc />
	public override Boolean Contains(Date date)
	{
		Int32 month = this.Month;

		if (date.Month == month && date.DayOfWeek == this.DayOfWeek)
		{
			Int32 maxDay = this.MaxDay;
			if (maxDay <= 0)
				maxDay += Date.UnsafeDaysInMonth(date.Year, date.Month);

			// Unoptimized:
			//   if (maxDay - date.Day >= 0 && maxDay - date.Day <= Date.DaysPerWeek - 1)
			// Optimized:
			if (unchecked((UInt32)(maxDay - date.Day) <= Date.DaysPerWeek - 1))
				return true;
		}

		return false;
	}

	/// <inheritdoc />
	public override IEnumerable<Date> EnumerateBackwardFrom(Date date)
	{
		if (date == Date.Empty)
			ThrowHelper.ThrowEmptyDateArgumentException(ExceptionArgument.date);
		return this.EnumerateBackwardFromIterator(date);
	}

	private IEnumerable<Date> EnumerateBackwardFromIterator(Date date)
	{
		Int32 year = date.Year;
		Date firstOccurrence = this.UnsafeGetOccurrence(year);
		if (firstOccurrence <= date)
			yield return firstOccurrence;
		--year;

		while (year >= Date.MinYear)
		{
			yield return this.UnsafeGetOccurrence(year);
			--year;
		}
	}

	/// <inheritdoc />
	public override IEnumerable<Date> EnumerateForwardFrom(Date date)
	{
		if (date == Date.Empty)
			ThrowHelper.ThrowEmptyDateArgumentException(ExceptionArgument.date);
		return this.EnumerateForwardFromIterator(date);
	}

	private IEnumerable<Date> EnumerateForwardFromIterator(Date date)
	{
		Int32 year = date.Year;
		Date firstOccurrence = this.UnsafeGetOccurrence(year);
		if (firstOccurrence >= date)
			yield return firstOccurrence;
		++year;

		while (year <= Date.MaxYear)
		{
			yield return this.UnsafeGetOccurrence(year);
			++year;
		}
	}

	/// <inheritdoc />
	public override Date GetOccurrence(Int32 year)
	{
		ThrowHelper.ThrowIfYearArgumentIsOutOfRange(year, ExceptionArgument.year);
		return this.UnsafeGetOccurrence(year);
	}

	private Date UnsafeGetOccurrence(Int32 year)
	{
		Debug.Assert(year >= Date.MinYear);
		Debug.Assert(year <= Date.MaxYear);

		Int32 month = this.Month;
		Int32 maxDay = this.MaxDay;
		if (maxDay <= 0)
			maxDay += Date.UnsafeDaysInMonth(year, month);
		Int32 dayOffset = this.DayOfWeek - Date.UnsafeDayOfWeek(year, month, maxDay);
		if (dayOffset > 0)
			dayOffset -= Date.DaysPerWeek;
		return Date.UnsafeCreate(year, month, maxDay + dayOffset);
	}
}
