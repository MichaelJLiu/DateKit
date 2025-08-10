using System;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for an event that occurs once every year on a specified instance of a specified
/// day of the week in a specified month.
/// </summary>
/// <threadsafety static="true" instance="true" />
public sealed class AnnualNthDayOfWeekOfMonthRecurrence : AnnualRecurrence
{
	/// <overloads>
	/// Creates an <see cref="AnnualNthDayOfWeekOfMonthRecurrence" />.
	/// </overloads>
	/// <summary>
	/// Creates an <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> using a specified month, day of the week,
	/// instance number, and the default <see cref="AnnualNthDayOfWeekOfMonthRecurrenceOptions" />.
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
	/// <returns>
	/// An <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> that occurs on <paramref name="instance" />
	/// of <paramref name="dayOfWeek" /> in <paramref name="month" />,
	/// using the default <see cref="AnnualNthDayOfWeekOfMonthRecurrenceOptions" />.
	/// </returns>
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
	/// The following example creates an <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> that occurs once every year
	/// on the last Monday of May.
	/// <code>
	/// var recurrence = AnnualNthDayOfWeekOfMonthRecurrence.Create(5, DayOfWeek.Monday, -1);
	/// </code>
	/// </example>
	public static AnnualNthDayOfWeekOfMonthRecurrence Create(Int32 month, DayOfWeek dayOfWeek, Int32 instance)
	{
		return Create(month, dayOfWeek, instance, AnnualNthDayOfWeekOfMonthRecurrenceOptions.Default);
	}

	/// <summary>
	/// Creates an <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> using a specified month, day of the week,
	/// instance number, and <see cref="AnnualNthDayOfWeekOfMonthRecurrenceOptions" />.
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
	/// <returns>
	/// An <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> that occurs on <paramref name="instance" />
	/// of <paramref name="dayOfWeek" /> in <paramref name="month" />, using <paramref name="options" />.
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
	/// <paramref name="dayOfWeek" /> is less than <see cref="DayOfWeek.Sunday" /> or greater than
	/// <see cref="DayOfWeek.Saturday" />.
	/// </para>
	/// <para>-or-</para>
	/// <para>
	/// <paramref name="instance" /> is less than -4, equal to 0, or greater than 4.
	/// </para>
	/// </exception>
	/// <remarks>
	/// The method makes a copy of <paramref name="options" />, so any subsequent changes to them will not affect
	/// the recurrence.
	/// </remarks>
	public static AnnualNthDayOfWeekOfMonthRecurrence Create(
		Int32 month, DayOfWeek dayOfWeek, Int32 instance, AnnualNthDayOfWeekOfMonthRecurrenceOptions options)
	{
		return new AnnualNthDayOfWeekOfMonthRecurrence(month, dayOfWeek, instance, options);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AnnualNthDayOfWeekOfMonthRecurrence" /> class.
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
	/// The constructor makes a copy of <paramref name="options" />, so any subsequent changes to them will not affect
	/// the recurrence.
	/// </remarks>
	private AnnualNthDayOfWeekOfMonthRecurrence(
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
		this.MaxDay = instance > 0
			? instance * Date.DaysPerWeek
			: Date.UncheckedDaysInMonth(month) + (instance + 1) * Date.DaysPerWeek;
		this.IncludeLeapDay = month == Date.February && instance < 0;
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

	/// <summary>
	/// Gets the latest possible day of the month on which the event can occur.
	/// </summary>
	private Int32 MaxDay { get; }

	private Boolean IncludeLeapDay { get; }

	/// <inheritdoc />
	protected internal override Boolean ContainsCore(Date date)
	{
		Int32 month = date.Month;
		if (month == this.Month)
		{
			Int32 year = date.Year;
			Int32 day = date.Day;
			if (Date.UncheckedDayOfWeek(year, month, day) == this.DayOfWeek)
			{
				Int32 maxDay = this.MaxDay;
				if (this.IncludeLeapDay && Date.UncheckedIsLeapYear(year))
					++maxDay;

				// Unoptimized:
				//   if (maxDay - day >= 0 && maxDay - day <= Date.DaysPerWeek - 1)
				// Optimized:
				if (unchecked((UInt32)(maxDay - day) <= Date.DaysPerWeek - 1))
					return year >= this.StartYear && year <= this.EndYear;
			}
		}

		return false;
	}

	/// <inheritdoc />
	protected internal override Date GetOccurrenceCore(Int32 year)
	{
		Int32 month = this.Month;
		Int32 maxDay = this.MaxDay;
		if (this.IncludeLeapDay && Date.UncheckedIsLeapYear(year))
			++maxDay;
		Int32 dayOffset = this.DayOfWeek - Date.UncheckedDayOfWeek(year, month, maxDay);
		if (dayOffset > 0)
			dayOffset -= Date.DaysPerWeek;
		return Date.UncheckedCreate(year, month, day: maxDay + dayOffset);
	}
}
