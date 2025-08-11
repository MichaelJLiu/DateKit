using System;

namespace DateKit.Schedules;

/// <summary>
/// Represents the schedule for Easter or an event that occurs a fixed number of days before or after Easter,
/// according to the Gregorian reckoning.
/// </summary>
/// <remarks>
/// The dates computed by this class are proleptic before 1583.
/// </remarks>
public sealed class GregorianEasterRecurrence : AnnualRecurrence
{
	/// <overloads>
	/// Creates a <see cref="GregorianEasterRecurrence" />.
	/// </overloads>
	/// <summary>
	/// Creates a <see cref="GregorianEasterRecurrence" /> using the default
	/// <see cref="GregorianEasterRecurrenceOptions" />.
	/// </summary>
	/// <returns>
	/// A <see cref="GregorianEasterRecurrence" />, using the default <see cref="GregorianEasterRecurrenceOptions" />.
	/// </returns>
	public static GregorianEasterRecurrence Create()
	{
		return new GregorianEasterRecurrence(GregorianEasterRecurrenceOptions.Default);
	}

	/// <summary>
	/// Creates a <see cref="GregorianEasterRecurrence" /> using specified 
	/// <see cref="GregorianEasterRecurrenceOptions" />.
	/// </summary>
	/// <param name="options">
	/// A <see cref="GregorianEasterRecurrenceOptions" /> instance that specifies options for the recurrence.
	/// </param>
	/// <returns>
	/// A <see cref="GregorianEasterRecurrence" />, using <paramref name="options" />.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="options" /> is <see langword="null" />.
	/// </exception>
	/// <remarks>
	/// The method makes a copy of <paramref name="options" />, so any subsequent changes to them will not affect
	/// the recurrence.
	/// </remarks>
	public static GregorianEasterRecurrence Create(GregorianEasterRecurrenceOptions options)
	{
#pragma warning disable CA1062 // False positive: Validate arguments of public methods (options)
		return new GregorianEasterRecurrence(options);
#pragma warning restore CA1062
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GregorianEasterRecurrence" /> class.
	/// </summary>
	/// <param name="options">
	/// A <see cref="GregorianEasterRecurrenceOptions" /> instance that specifies options for the recurrence.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="options" /> is <see langword="null" />.
	/// </exception>
	/// <remarks>
	/// The constructor makes a copy of <paramref name="options" />, so any subsequent changes to them will not affect
	/// the recurrence.
	/// </remarks>
	private GregorianEasterRecurrence(GregorianEasterRecurrenceOptions options)
		: base(options)
	{
		this.Offset = options.Offset;
	}

	/// <summary>
	/// Gets the offset in days of the event from Easter.
	/// </summary>
	/// <value>
	/// An integer between -80 and 250 that specifies the number of days between Easter and the event.
	/// The default is zero.
	/// </value>
	public Int32 Offset { get; }

	/// <inheritdoc />
	protected internal override Date GetOccurrenceCore(Int32 year)
	{
		// The date of Easter is based on the "ecclesiastical moon" (an approximation of the astronomical moon),
		// whose phases recur on the same dates every 19 years. Calculate the year's position in the current cycle:
		const Int32 yearsPerCycle = 19;
		// Unoptimized:
		//   Int32 yearOfCycle = year % yearsPerCycle;
		// Optimized (valid for year in [0..0x13B13B13]):
		const Int32 multiplier1 = (Int32)((1L << 32) / yearsPerCycle) + 1;
		Int32 yearOfCycle = (Int32)((unchecked((UInt32)(year * multiplier1)) * (Int64)yearsPerCycle) >>> 32);

		// Calculate the number of days between March 21 and the "paschal full moon", the ecclesiastical full moon
		// that occurs within the 29-day period between March 21 and April 18.
		//
		// 1. Because the solar year is 11 days longer than the lunar year and intercalary months consist of 30 days,
		//    add -11 mod 30 = 19 days per year:
		const Int32 daysPerLunarMonth = 30;
		const Int32 daysPerShortLunarMonth = 29;
		const Int32 daysPerLunarYear = (daysPerLunarMonth + daysPerShortLunarMonth) * 6; // 354
		const Int32 daysPerSolarYear = Date.DaysPerYear; // 365
		Int32 daysToFullMoon = yearOfCycle * (daysPerLunarMonth - (daysPerSolarYear - daysPerLunarYear));

		// 2. Restore the leap days omitted by the Gregorian calendar in years that are a multiple of 100 but not 400:
		Int32 century = Date.GetCentury(year);
		Int32 centuryLeapDays = ((century + 1) * 3) >>> 2; // equal to (century - century / 4)
		daysToFullMoon += centuryLeapDays;

		// 3. Because the astronomical lunar month is slightly shorter than the average ecclesiastical lunar month,
		//    shift the paschal full moon back by one day eight times every 2,500 years, at seven intervals of 300 years
		//    followed by one interval of 400 years, starting one cycle in the year 1800:
		// Unoptimized:
		//   daysToFullMoon -= (century * 8 + 13) / 25;
		// Optimized (valid for century in [0..141]; 7 is minimum shift count that encompasses [0..99]):
		const Int32 shift2 = 7;
		const Int32 multiplier2 = (8 << shift2) / 25 + 1;
		const Int32 increment2 = 66; // Range(0, 25).Max(c => (((c * 8 + 13) / 25) << shift2) - c * multiplier2)
		daysToFullMoon -= (century * multiplier2 + increment2) >>> shift2;

		// 4. Reduce the value to the range [0..29] and ensure that it equals 16 (for April 6) in the year 1583:
		daysToFullMoon += 15;
		// Unoptimized:
		//   daysToFullMoon %= daysPerLunarMonth;
		// Optimized (valid for daysToFullMoon in [0..0x12492492]):
		const Int32 multiplier3 = (Int32)((1L << 32) / daysPerLunarMonth) + 1;
		daysToFullMoon = (Int32)((unchecked((UInt32)(daysToFullMoon * multiplier3)) * (Int64)daysPerLunarMonth) >>> 32);

		// 5. Subtract one if the value equals 29 (for April 19), because by tradition Easter should occur no later than
		//    April 25, or if the value equals 28 (for April 18) and the year is the 12th or later of the current cycle,
		//    so that the phases of the ecclesiastical moon do not occur, inelegantly, on the same dates as those from
		//    11 years earlier:
		if (daysToFullMoon >= 28 && (daysToFullMoon > 28 || yearOfCycle > 10))
			--daysToFullMoon;

		// Calculate the day of the week of the paschal full moon:
		Int32 dayOfWeek = daysToFullMoon
			+ ((year * 5) >>> 2) // Add one day every common year, two days every leap year; equal to (year + year / 4).
			- centuryLeapDays // Subtract the leap days omitted by the Gregorian calendar.
			+ 2; // Synchronize with the Gregorian calendar.
		// Unoptimized:
		//   dayOfWeek %= Date.DaysPerWeek;
		// Optimized (valid for dayOfWeek in [0..0x55555555]):
		const Int32 multiplier4 = (Int32)((1L << 32) / Date.DaysPerWeek) + 1;
		dayOfWeek = (Int32)((unchecked((UInt32)(dayOfWeek * multiplier4)) * (Int64)Date.DaysPerWeek) >>> 32);

		// Calculate the day of Easter, which occurs on the Sunday following the paschal full moon:
		Int32 day = 21 // Start on March 21.
			+ daysToFullMoon // Advance to the paschal full moon.
			+ Date.DaysPerWeek - dayOfWeek // Advance to the following Sunday.
			+ this.Offset;
		return day <= Date.DaysInMarch
			? Date.UncheckedCreate(year, Date.March, day)
			: Date.UncheckedCreate(year, Date.April, day - Date.DaysInMarch);
	}
}
