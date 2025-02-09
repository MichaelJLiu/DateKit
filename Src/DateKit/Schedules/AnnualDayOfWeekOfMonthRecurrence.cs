using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DateKit.Schedules;

using Options = AnnualDayOfWeekOfMonthRecurrenceOptions;

/// <summary>
/// Represents the schedule for an event that occurs once every year on a specified instance of a specified
/// day of the week in a specified month.
/// </summary>
/// <threadsafety static="true" instance="true" />
public sealed class AnnualDayOfWeekOfMonthRecurrence : AnnualRecurrence
{
	private readonly Int32 _maxDay; // The latest possible day of the month on which the event can occur.
	private readonly Boolean _includeLeapDay;

	/// <summary>
	/// Initializes a new instance of the <see cref="AnnualDayOfWeekOfMonthRecurrence" /> class.
	/// </summary>
	/// <param name="month">
	/// An integer between 1 and 12 that specifies the month of the event.
	/// </param>
	/// <param name="dayOfWeek">
	/// The <see cref="System.DayOfWeek" /> that specifies the day of the week of the event.
	/// </param>
	/// <param name="instance">
	/// A nonzero integer between -4 and 4 that specifies the first, second, third, or fourth instance
	/// of the <paramref name="dayOfWeek" /> in the <paramref name="month" />. A positive integer counts
	/// from the beginning of the month, whereas a negative integer counts from the end of the month.
	/// </param>
	/// <param name="options">
	/// An <see cref="Options" /> instance that specifies options for the recurrence,
	/// or <see langword="null" /> to use the default options. The default is <see langword="null" />.
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
	/// <paramref name="instance" /> is less than -4, equal to 0, or greater than 4.
	/// </para>
	/// </exception>
	/// <remarks>
	/// The constructor takes a snapshot of the <paramref name="options" />, so any subsequent changes to them
	/// will not affect the recurrence.
	/// </remarks>
	/// <example>
	/// The expression <c>new AnnualDayOfWeekOfMonthRecurrence(5, DayOfWeek.Monday, -1)</c> represents the schedule
	/// for an event that occurs once every year on the last Monday of May.
	/// </example>
	public AnnualDayOfWeekOfMonthRecurrence(Int32 month, DayOfWeek dayOfWeek, Int32 instance, Options? options = null)
		: base(options ?? Options.Default)
	{
		ThrowHelper.ThrowIfMonthArgumentIsOutOfRange(month, ExceptionArgument.month);
		// Unoptimized:
		//   if (dayOfWeek < DayOfWeek.Sunday || dayOfWeek > DayOfWeek.Saturday)
		// Optimized:
		if (unchecked((UInt32)(dayOfWeek - DayOfWeek.Sunday)) > DayOfWeek.Saturday - DayOfWeek.Sunday)
			throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null);
		// Unoptimized:
		//   if (instance == 0 || instance < -4 || instance > 4)
		// Optimized:
		if (instance == 0 || unchecked((UInt32)(instance + 4)) > 8)
			throw new ArgumentOutOfRangeException(nameof(instance), instance, null);

		this.Month = month;
		this.DayOfWeek = dayOfWeek;
		this.Instance = instance;
		_maxDay = instance > 0
			? instance * Date.DaysPerWeek
			: Date.UncheckedDaysInMonth(month) + (instance + 1) * Date.DaysPerWeek;
		_includeLeapDay = month == Date.February && instance < 0;
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
	/// Gets an integer that specifies the instance of the day of the week of the event.
	/// </summary>
	/// <value>
	/// A nonzero integer between -4 and 4 that specifies the first, second, third, or fourth instance
	/// of the <see cref="DayOfWeek" /> in the <see cref="Month" />. A positive integer counts
	/// from the beginning of the month, whereas a negative integer counts from the end of the month.
	/// </value>
	public Int32 Instance { get; }

	/// <inheritdoc />
	public override Boolean Contains(Date date)
	{
		Int32 year = date.Year;

		if (year >= this.StartYear && year <= this.EndYear)
		{
			Int32 month = date.Month;

			if (month == this.Month && date.DayOfWeek == this.DayOfWeek)
			{
				Int32 maxDay = _maxDay;
				if (_includeLeapDay && Date.UnsafeIsLeapYear(year))
					++maxDay;

				// Unoptimized:
				//   if (maxDay - date.Day >= 0 && maxDay - date.Day <= Date.DaysPerWeek - 1)
				// Optimized:
				if (unchecked((UInt32)(maxDay - date.Day) <= Date.DaysPerWeek - 1))
					return true;
			}
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
		Int32 startYear = this.StartYear;
		Int32 year = date.Year;
		if (year < startYear)
			yield break;

		Int32 endYear = this.EndYear;
		if (year > endYear)
			year = endYear;

		Date firstOccurrence = this.UnsafeGetOccurrence(year);
		if (firstOccurrence <= date)
			yield return firstOccurrence;

		for (--year; year >= startYear; --year)
			yield return this.UnsafeGetOccurrence(year);
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
		Int32 endYear = this.EndYear;
		Int32 year = date.Year;
		if (year > endYear)
			yield break;

		Int32 startYear = this.StartYear;
		if (year < startYear)
			year = startYear;

		Date firstOccurrence = this.UnsafeGetOccurrence(year);
		if (firstOccurrence >= date)
			yield return firstOccurrence;

		for (++year; year <= endYear; ++year)
			yield return this.UnsafeGetOccurrence(year);
	}

	/// <inheritdoc />
	public override Date GetOccurrence(Int32 year)
	{
		return year >= this.StartYear && year <= this.EndYear
			? this.UnsafeGetOccurrence(year)
			: Date.Empty;
	}

	private Date UnsafeGetOccurrence(Int32 year)
	{
		Debug.Assert(year >= this.StartYear);
		Debug.Assert(year <= this.EndYear);

		Int32 month = this.Month;
		Int32 maxDay = _maxDay;
		if (_includeLeapDay && Date.UnsafeIsLeapYear(year))
			++maxDay;
		Int32 dayOffset = this.DayOfWeek - Date.UnsafeDayOfWeek(year, month, maxDay);
		if (dayOffset > 0)
			dayOffset -= Date.DaysPerWeek;
		return Date.UnsafeCreate(year, month, day: maxDay + dayOffset);
	}
}
