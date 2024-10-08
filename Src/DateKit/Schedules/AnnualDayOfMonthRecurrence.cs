using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DateKit.Schedules;

using Options = AnnualDayOfMonthRecurrenceOptions;

/// <summary>
/// Represents the schedule for an event that occurs once every year on a specified day of a specified month.
/// </summary>
/// <threadsafety static="true" instance="true" />
public sealed class AnnualDayOfMonthRecurrence : AnnualRecurrence
{
	private readonly Int32 _startYear;
	private readonly Int32 _endYear;
	private readonly Int32 _month;
	private readonly Int32 _day;
	private readonly SByte[] _adjustments;
	private readonly Boolean _mayOccurInPreviousYear;
	private readonly Boolean _mayOccurInNextYear;

	/// <summary>
	/// Initializes a new instance of the <see cref="AnnualDayOfMonthRecurrence" /> class.
	/// </summary>
	/// <param name="month">
	/// An integer between 1 and 12 that specifies the month of the event.
	/// </param>
	/// <param name="day">
	/// An integer between 1 and 31 that specifies the day of the month of the event.
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
	/// <paramref name="day" /> is less than 1 or greater than the number of days
	/// in the specified <paramref name="month" /> (28 for February).
	/// </para>
	/// </exception>
	/// <remarks>
	/// The constructor takes a snapshot of the <paramref name="options" />, so any subsequent changes to them
	/// will not affect the recurrence.
	/// </remarks>
	public AnnualDayOfMonthRecurrence(Int32 month, Int32 day, Options? options = null)
	{
		ThrowHelper.ThrowIfMonthArgumentIsOutOfRange(month, ExceptionArgument.month);
		ThrowHelper.ThrowIfDayArgumentIsOutOfRange(month, day, ExceptionArgument.day);

		options ??= Options.Default;
		_startYear = options.StartYear;
		_endYear = options.EndYear;
		_month = month;
		_day = day;

		DayOfWeekAdjustments? adjustments = options.DayOfWeekAdjustments;
		if (adjustments != null)
		{
			SByte[] array = adjustments.ToArray();
			_adjustments = array;
			_mayOccurInPreviousYear = month == Date.January && day + array.Min() < 1;
			_mayOccurInNextYear = month == Date.December && day + array.Max() > 31;
		}
		else
			_adjustments = DayOfWeekAdjustments.EmptyArray;
	}

	/// <inheritdoc />
	public override Int32 StartYear => _startYear;

	/// <inheritdoc />
	public override Int32 EndYear => _endYear;

	/// <summary>
	/// Gets the month of the event.
	/// </summary>
	/// <value>
	/// An integer between 1 and 12 that specifies the month of the event.
	/// </value>
	public Int32 Month => _month;

	/// <summary>
	/// Gets the day of the month of the event.
	/// </summary>
	/// <value>
	/// An integer between 1 and 31 that specifies the day of the month of the event.
	/// </value>
	public Int32 Day => _day;

	/// <summary>
	/// Gets the adjustments to make to the <see cref="AnnualDayOfMonthRecurrence.Day" />
	/// of the <see cref="AnnualDayOfMonthRecurrence" /> when it falls on particular days of the week.
	/// </summary>
	/// <value>
	/// A <see cref="Schedules.DayOfWeekAdjustments" /> instance.
	/// </value>
	public DayOfWeekAdjustments DayOfWeekAdjustments => new(_adjustments);

	/// <inheritdoc />
	public override Boolean Contains(Date date)
	{
		Int32 year = date.Year;
		return year >= _startYear && year <= _endYear && (
			date == this.UnsafeGetOccurrence(year) ||
			(_mayOccurInPreviousYear && year < Date.MaxYear && date == this.UnsafeGetOccurrence(year + 1)) ||
			(_mayOccurInNextYear && year > Date.MinYear && date == this.UnsafeGetOccurrence(year - 1)));
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
		Int32 startYear = _startYear;
		Int32 year = date.Year;
		if (year < startYear)
			yield break;

		Int32 endYear = _endYear;
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
		Int32 endYear = _endYear;
		Int32 year = date.Year;
		if (year > endYear)
			yield break;

		Int32 startYear = _startYear;
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
		return year >= _startYear && year <= _endYear
			? this.UnsafeGetOccurrence(year)
			: Date.Empty;
	}

	private Date UnsafeGetOccurrence(Int32 year)
	{
		Debug.Assert(year >= _startYear);
		Debug.Assert(year <= _endYear);

		Int32 month = _month;
		Int32 day = _day;
		Date occurrence = Date.UnsafeCreate(year, month, day);
		Int32 adjustment = _adjustments[(Int32)Date.UnsafeDayOfWeek(year, month, day)];
		if (adjustment != 0)
			occurrence = Date.AddSmallDays(occurrence, adjustment);
		return occurrence;
	}
}
