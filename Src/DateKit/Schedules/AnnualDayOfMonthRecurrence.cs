using System;
using System.Collections.Generic;
using System.Linq;

namespace DateKit.Schedules;

using Options = AnnualDayOfMonthRecurrenceOptions;

/// <summary>
/// Represents the schedule for an event that occurs once every year on a specified day of a specified month.
/// </summary>
/// <threadsafety static="true" instance="true" />
public sealed class AnnualDayOfMonthRecurrence : AnnualRecurrence
{
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
		: base(options ??= Options.Default)
	{
		ThrowHelper.ThrowIfMonthArgumentIsOutOfRange(month, ExceptionArgument.month);
		ThrowHelper.ThrowIfDayArgumentIsOutOfRange(month, day, ExceptionArgument.day);

		this.Month = month;
		this.Day = day;

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

	/// <summary>
	/// Gets the month of the event.
	/// </summary>
	/// <value>
	/// An integer between 1 and 12 that specifies the month of the event.
	/// </value>
	public Int32 Month { get; }

	/// <summary>
	/// Gets the day of the month of the event.
	/// </summary>
	/// <value>
	/// An integer between 1 and 31 that specifies the day of the month of the event.
	/// </value>
	public Int32 Day { get; }

	/// <summary>
	/// Gets the adjustments to make to the <see cref="AnnualDayOfMonthRecurrence.Day" />
	/// of the <see cref="AnnualDayOfMonthRecurrence" /> when it falls on particular days of the week.
	/// </summary>
	/// <value>
	/// A <see cref="Schedules.DayOfWeekAdjustments" /> instance.
	/// </value>
	public DayOfWeekAdjustments DayOfWeekAdjustments => new(_adjustments);

	/// <inheritdoc />
	public override void GetDayOfYearRange(out Int32 minDayOfYear, out Int32 maxDayOfYear)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	protected internal override Boolean ContainsCore(Date date)
	{
		Int32 year = date.Year;
		return year >= this.StartYear && year <= this.EndYear && (
			date == this.GetOccurrenceCore(year) ||
			(_mayOccurInPreviousYear && year < Date.MaxYear && date == this.GetOccurrenceCore(year + 1)) ||
			(_mayOccurInNextYear && year > Date.MinYear && date == this.GetOccurrenceCore(year - 1)));
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateBackwardFromCore(Date date)
	{
		Int32 startYear = this.StartYear;
		Int32 year = date.Year;
		if (year < startYear)
			yield break;

		Int32 endYear = this.EndYear;
		if (year > endYear)
			year = endYear;

		Date firstOccurrence = this.GetOccurrenceCore(year);
		if (firstOccurrence <= date)
			yield return firstOccurrence;

		for (--year; year >= startYear; --year)
			yield return this.GetOccurrenceCore(year);
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateForwardFromCore(Date date)
	{
		Int32 endYear = this.EndYear;
		Int32 year = date.Year;
		if (year > endYear)
			yield break;

		if (_mayOccurInNextYear && date.Month == Date.January)
			--year;

		Int32 startYear = this.StartYear;
		if (year < startYear)
			year = startYear;

		Date firstOccurrence = this.GetOccurrenceCore(year);
		if (firstOccurrence >= date)
			yield return firstOccurrence;

		for (++year; year <= endYear; ++year)
			yield return this.GetOccurrenceCore(year);
	}

	/// <inheritdoc />
	protected internal override Date GetOccurrenceCore(Int32 year)
	{
		Int32 month = this.Month;
		Int32 day = this.Day;
		Date occurrence = Date.UncheckedCreate(year, month, day);
		Int32 adjustment = _adjustments[(Int32)Date.UncheckedDayOfWeek(year, month, day)];
		if (adjustment != 0)
			occurrence = Date.AddSmallDays(occurrence, adjustment);
		return occurrence;
	}
}
