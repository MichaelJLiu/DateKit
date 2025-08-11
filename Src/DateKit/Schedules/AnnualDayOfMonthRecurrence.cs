using System;
using System.Collections.Generic;
using System.Linq;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for an event that occurs once every year on a specified day of a specified month.
/// </summary>
/// <threadsafety static="true" instance="true" />
public sealed class AnnualDayOfMonthRecurrence : AnnualRecurrence
{
	private readonly SByte[] _adjustments;

	/// <overloads>
	/// Creates an <see cref="AnnualDayOfMonthRecurrence" />.
	/// </overloads>
	/// <summary>
	/// Creates an <see cref="AnnualDayOfMonthRecurrence" /> using a specified month, day of the month,
	/// and the default <see cref="AnnualDayOfMonthRecurrenceOptions" />.
	/// </summary>
	/// <param name="month">
	/// An integer between 1 and 12 that specifies the month in which the event occurs.
	/// </param>
	/// <param name="day">
	/// An integer between 1 and 31 that specifies the day of the month on which the event occurs.
	/// </param>
	/// <returns>
	/// An <see cref="AnnualDayOfMonthRecurrence" /> that occurs on <paramref name="day" /> of
	/// <paramref name="month" />, using the default <see cref="AnnualDayOfMonthRecurrenceOptions" />.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <para>
	/// <paramref name="month" /> is less than 1 or greater than 12.
	/// </para>
	/// <para>-or-</para>
	/// <para>
	/// <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />
	/// (28 for February).
	/// </para>
	/// </exception>
	public static AnnualDayOfMonthRecurrence Create(Int32 month, Int32 day)
	{
		return Create(month, day, AnnualDayOfMonthRecurrenceOptions.Default);
	}

	/// <summary>
	/// Creates an <see cref="AnnualDayOfMonthRecurrence" /> using a specified month, day of the month,
	/// and <see cref="AnnualDayOfMonthRecurrenceOptions" />.
	/// </summary>
	/// <param name="month">
	/// An integer between 1 and 12 that specifies the month in which the event occurs.
	/// </param>
	/// <param name="day">
	/// An integer between 1 and 31 that specifies the day of the month on which the event occurs.
	/// </param>
	/// <param name="options">
	/// An <see cref="AnnualDayOfMonthRecurrenceOptions" /> instance that specifies options for the recurrence.
	/// </param>
	/// <returns>
	/// An <see cref="AnnualDayOfMonthRecurrence" /> that occurs on <paramref name="day" /> of
	/// <paramref name="month" />, using <paramref name="options" />.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="options" /> is <see langword="null" />.
	/// </exception>
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
	/// The method makes a copy of <paramref name="options" />, so any subsequent changes to them will not affect
	/// the recurrence.
	/// </remarks>
	public static AnnualDayOfMonthRecurrence Create(Int32 month, Int32 day, AnnualDayOfMonthRecurrenceOptions options)
	{
#pragma warning disable CA1062 // False positive: Validate arguments of public methods (options)
		return new AnnualDayOfMonthRecurrence(month, day, options);
#pragma warning restore CA1062
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AnnualDayOfMonthRecurrence" /> class.
	/// </summary>
	/// <param name="month">
	/// An integer between 1 and 12 that specifies the month in which the event occurs.
	/// </param>
	/// <param name="day">
	/// An integer between 1 and 31 that specifies the day of the month on which the event occurs.
	/// </param>
	/// <param name="options">
	/// An <see cref="AnnualDayOfMonthRecurrenceOptions" /> instance that specifies options for the recurrence.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="options" /> is <see langword="null" />.
	/// </exception>
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
	/// The constructor makes a copy of <paramref name="options" />, so any subsequent changes to them will not affect
	/// the recurrence.
	/// </remarks>
	private AnnualDayOfMonthRecurrence(Int32 month, Int32 day, AnnualDayOfMonthRecurrenceOptions options)
		: base(options)
	{
		ThrowHelper.ThrowIfMonthArgumentIsOutOfRange(month, ExceptionArgument.month);
		ThrowHelper.ThrowIfDayArgumentIsOutOfRange(month, day, ExceptionArgument.day);

		this.Month = month;
		this.Day = day;

		DayOfWeekAdjustments? adjustments = options.DayOfWeekAdjustments;
		_adjustments = adjustments != null
			? adjustments.ToArray()
			: DayOfWeekAdjustments.DefaultArray;

		// ReSharper disable PatternAlwaysMatches
		this.MinPackedMonthDayInPreviousYear =
			month == Date.January && day + _adjustments.Min() is Int32 minDay and <= 0
				? Date.PackMonthDay(Date.December, Date.DaysInDecember) + minDay
				: Date.PackMonthDay(Date.December, Date.DaysInDecember) + 1;
		this.MaxPackedMonthDayInNextYear =
			month == Date.December && day + _adjustments.Max() - (Date.DaysInDecember + 1) is Int32 maxDay and >= 0
				? Date.PackMonthDay(Date.January, 1) + maxDay
				: Date.PackMonthDay(Date.January, 1) - 1;
		// ReSharper restore PatternAlwaysMatches
	}

	/// <summary>
	/// Gets the month in which the event occurs.
	/// </summary>
	/// <value>
	/// An integer between 1 and 12 that specifies the month in which the event occurs.
	/// </value>
	public Int32 Month { get; }

	/// <summary>
	/// Gets the day of the month on which the event occurs.
	/// </summary>
	/// <value>
	/// An integer between 1 and 31 that specifies the day of the month on which the event occurs.
	/// </value>
	public Int32 Day { get; }

	/// <summary>
	/// Gets the adjustments to make to the <see cref="Day" /> of the <see cref="AnnualDayOfMonthRecurrence" />
	/// when it falls on particular days of the week.
	/// </summary>
	/// <value>
	/// A <see cref="Schedules.DayOfWeekAdjustments" /> instance.
	/// </value>
	public DayOfWeekAdjustments DayOfWeekAdjustments => new(_adjustments);

	private Int32 MinPackedMonthDayInPreviousYear { get; }

	private Int32 MaxPackedMonthDayInNextYear { get; }

	/// <inheritdoc />
	protected internal override Boolean ContainsCore(Date date)
	{
		Int32 year = date.Year;
		Int32 packedMonthDay = date.PackedMonthDay;
		if (packedMonthDay <= this.MaxPackedMonthDayInNextYear)
			--year;
		else if (packedMonthDay >= this.MinPackedMonthDayInPreviousYear)
			++year;
		return this.ContainsCore(date, year);
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateBackwardFromCore(Date date)
	{
		Int32 year = date.Year;
		if (date.PackedMonthDay >= this.MinPackedMonthDayInPreviousYear)
			++year;
		return this.EnumerateBackwardFromCore(date, year);
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateForwardFromCore(Date date)
	{
		Int32 year = date.Year;
		if (date.PackedMonthDay <= this.MaxPackedMonthDayInNextYear)
			--year;
		return this.EnumerateForwardFromCore(date, year);
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
