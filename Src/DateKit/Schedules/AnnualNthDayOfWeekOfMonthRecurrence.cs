using System;
using System.Collections.Generic;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for an event that occurs once every year on a specified instance of a specified
/// day of the week in a specified month.
/// </summary>
/// <threadsafety static="true" instance="true" />
public sealed class AnnualNthDayOfWeekOfMonthRecurrence : AnnualRecurrence
{
	private readonly Int32 _maxDay; // The latest possible day of the month on which the event can occur.
	private readonly Boolean _includeLeapDay;

	/// <overloads>
	/// Initializes a new instance of the <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> class.
	/// </overloads>
	/// <summary>
	/// Initializes a new instance of the <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> class with the default
	/// options.
	/// </summary>
	/// <param name="month">
	/// An integer between 1 and 12 that specifies the month in which the event occurs.
	/// </param>
	/// <param name="dayOfWeek">
	/// The <see cref="System.DayOfWeek" /> that specifies the day of the week on which the event occurs.
	/// </param>
	/// <param name="instance">
	/// A nonzero integer between -4 and 4 that specifies whether the event occurs on the first, second, third,
	/// or fourth instance of the <paramref name="dayOfWeek" /> in the <paramref name="month" />. A positive integer
	/// counts from the beginning of the month, whereas a negative integer counts from the end of the month.
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
	/// <example>
	/// The following example creates an <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> that represents the schedule
	/// for an event that occurs once every year on the last Monday of May.
	/// <code>
	/// AnnualNthDayOfWeekOfMonthRecurrence recurrence = new(5, DayOfWeek.Monday, -1);
	/// </code>
	/// </example>
	public AnnualNthDayOfWeekOfMonthRecurrence(Int32 month, DayOfWeek dayOfWeek, Int32 instance)
		: this(month, dayOfWeek, instance, AnnualNthDayOfWeekOfMonthRecurrenceOptions.Default)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> class with specified
	/// options.
	/// </summary>
	/// <param name="month">
	/// An integer between 1 and 12 that specifies the month in which the event occurs.
	/// </param>
	/// <param name="dayOfWeek">
	/// The <see cref="System.DayOfWeek" /> that specifies the day of the week on which the event occurs.
	/// </param>
	/// <param name="instance">
	/// A nonzero integer between -4 and 4 that specifies whether the event occurs on the first, second, third,
	/// or fourth instance of the <paramref name="dayOfWeek" /> in the <paramref name="month" />. A positive integer
	/// counts from the beginning of the month, whereas a negative integer counts from the end of the month.
	/// </param>
	/// <param name="options">
	/// An <see cref="AnnualNthDayOfWeekOfMonthRecurrenceOptions" /> instance that specifies options for the recurrence.
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
	/// <paramref name="dayOfWeek" /> is less than <see cref="DayOfWeek.Sunday" /> or greater than
	/// <see cref="DayOfWeek.Saturday" />.
	/// </para>
	/// <para>-or-</para>
	/// <para>
	/// <paramref name="instance" /> is less than -4, equal to 0, or greater than 4.
	/// </para>
	/// </exception>
	/// <remarks>
	/// The constructor makes a copy of the <paramref name="options" />, so any subsequent changes to them
	/// will not affect the recurrence.
	/// </remarks>
	public AnnualNthDayOfWeekOfMonthRecurrence(
		Int32 month, DayOfWeek dayOfWeek, Int32 instance, AnnualNthDayOfWeekOfMonthRecurrenceOptions options)
		: base(options)
	{
		ThrowHelper.ThrowIfMonthArgumentIsOutOfRange(month, ExceptionArgument.month);

		if (dayOfWeek is < DayOfWeek.Sunday or > DayOfWeek.Saturday)
		{
			throw new ArgumentOutOfRangeException(
				nameof(dayOfWeek), dayOfWeek, "The value specifies an invalid day of the week.");
		}

		if (instance is 0 or < -4 or > 4)
		{
			throw new ArgumentOutOfRangeException(
				nameof(instance), instance, "The value must be a nonzero integer between -4 and 4.");
		}

		this.Month = month;
		this.DayOfWeek = dayOfWeek;
		this.Instance = instance;
		_maxDay = instance > 0
			? instance * Date.DaysPerWeek
			: Date.UncheckedDaysInMonth(month) + (instance + 1) * Date.DaysPerWeek;
		_includeLeapDay = month == Date.February && instance < 0;
	}

	/// <summary>
	/// Gets the month in which the event occurs.
	/// </summary>
	/// <value>
	/// An integer between 1 and 12 that specifies the month in which the event occurs.
	/// </value>
	public Int32 Month { get; }

	/// <summary>
	/// Gets the day of the week on which the event occurs.
	/// </summary>
	/// <value>
	/// The <see cref="System.DayOfWeek" /> that specifies the day of the week on which the event occurs.
	/// </value>
	public DayOfWeek DayOfWeek { get; }

	/// <summary>
	/// Gets an integer that specifies the instance of the day of the week of the event.
	/// </summary>
	/// <value>
	/// A nonzero integer between -4 and 4 that specifies whether the event occurs on the first, second, third,
	/// or fourth instance of the <see cref="DayOfWeek" /> in the <see cref="Month" />. A positive integer
	/// counts from the beginning of the month, whereas a negative integer counts from the end of the month.
	/// </value>
	public Int32 Instance { get; }

	/// <inheritdoc />
	public override void GetDayOfYearRange(out Int32 minDayOfYear, out Int32 maxDayOfYear)
	{
		Int32 dayOfYear = Date.UncheckedDayOfYear(this.Month, _maxDay);
		minDayOfYear = dayOfYear - (Date.DaysPerWeek - 1);
		maxDayOfYear = dayOfYear + (_includeLeapDay ? 1 : 0);
	}

	/// <inheritdoc />
	protected internal override Boolean ContainsCore(Date date)
	{
		Int32 year = date.Year;

		if (year >= this.StartYear && year <= this.EndYear)
		{
			Int32 month = date.Month;

			if (month == this.Month)
			{
				Int32 day = date.Day;

				if (Date.UncheckedDayOfWeek(year, month, day) == this.DayOfWeek)
				{
					Int32 maxDay = _maxDay;
					if (_includeLeapDay && Date.UncheckedIsLeapYear(year))
						++maxDay;

					// Unoptimized:
					//   if (maxDay - day >= 0 && maxDay - day <= Date.DaysPerWeek - 1)
					// Optimized:
					if (unchecked((UInt32)(maxDay - day) <= Date.DaysPerWeek - 1))
						return true;
				}
			}
		}

		return false;
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateBackwardFromCore(Date date)
	{
		Int32 year = date.Year;
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
		Int32 year = date.Year;
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

	/// <inheritdoc />
	protected internal override Date GetOccurrenceCore(Int32 year)
	{
		Int32 month = this.Month;
		Int32 maxDay = _maxDay;
		if (_includeLeapDay && Date.UncheckedIsLeapYear(year))
			++maxDay;
		Int32 dayOffset = this.DayOfWeek - Date.UncheckedDayOfWeek(year, month, maxDay);
		if (dayOffset > 0)
			dayOffset -= Date.DaysPerWeek;
		return Date.UncheckedCreate(year, month, day: maxDay + dayOffset);
	}
}
