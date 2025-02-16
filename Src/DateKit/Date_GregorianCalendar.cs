using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DateKit;

partial struct Date
{
	internal const Int32 YearsPerCentury = 100;

	/// <summary>
	/// Represents the number of months in a year.
	/// </summary>
	/// <remarks>
	/// The value of this constant is 12.
	/// </remarks>
	public const Int32 MonthsPerYear = 12;

	internal const Int32 January = 1;
	internal const Int32 February = 2;
	internal const Int32 December = 12;

	/// <summary>
	/// Represents the number of days in a common (non-leap) year.
	/// </summary>
	/// <remarks>
	/// The value of this constant is 365.
	/// </remarks>
	public const Int32 DaysPerYear = 365;

	private const Int32 DaysPer4Years = DaysPerYear * 4 + 1; // one leap day every four years
	private const Int32 DaysPer100Years = DaysPer4Years * 25 - 1; // ...except every 100 years
	private const Int32 DaysPer400Years = DaysPer100Years * 4 + 1; // ...except every 400 years

	internal const Int32 MinDaysPerMonth = 28;
	internal const Int32 MaxDaysPerMonth = 31;

	private const Int32 DaysInJanuary = 31;
	private const Int32 DaysInFebruary = 28;
	private const Int32 LeapDay = 29;
	private const Int32 DaysInDecember = 31;

	/// <summary>
	/// Represents the number of days in a week.
	/// </summary>
	/// <remarks>
	/// The value of this constant is 7.
	/// </remarks>
	public const Int32 DaysPerWeek = 7;

#if NET8_0_OR_GREATER && DATEKIT_LOOKUP_TABLES
	private struct YearDataItem
	{
		// The number of days between January 1, 0001, and March 1 of this year, minus 1 to compensate
		// for adding 1-based day values. The number is negative in year 0.
		public Int32 DaysInPreviousYears;

		// The DayOfWeekMonthData starting offset for this year, equal to the day of the week of January 1
		// of this year, plus DaysPerWeek if this is a leap year, multiplied by MonthsPerYear.
		public Byte DayOfWeekMonthDataOffset;
	}

	[InlineArray(MaxYear + 1)]
	private struct YearDataArray
	{
#pragma warning disable IDE0051 // unused private member
		private YearDataItem _0;
#pragma warning restore IDE0051
	}

	private static readonly YearDataArray s_yearData = CreateYearData();

	private static YearDataArray CreateYearData()
	{
		YearDataArray data = new();
		Int32 daysInPreviousYears =
			-(DaysPerYear - DaysInJanuary - DaysInFebruary + 1); // January 1, 0001, to March 1, 0000, minus one
		Int32 firstDayOfWeek = 1; // Monday, January 1, 0001
		data[0].DaysInPreviousYears = daysInPreviousYears;

		for (Int32 year = 1; year <= MaxYear; ++year)
		{
			Boolean isLeapYear = UncheckedIsLeapYear(year);

			daysInPreviousYears += DaysPerYear + (isLeapYear ? 1 : 0);
			data[year].DaysInPreviousYears = daysInPreviousYears;
			data[year].DayOfWeekMonthDataOffset =
				(Byte)((firstDayOfWeek + (isLeapYear ? DaysPerWeek : 0)) * MonthsPerYear);

			firstDayOfWeek += 1 + (isLeapYear ? 1 : 0);
			if (firstDayOfWeek >= DaysPerWeek)
				firstDayOfWeek -= DaysPerWeek;
		}

		return data;
	}

	private static ReadOnlySpan<Byte> DayOfWeekMonthData =>
		[
			0, // unused
			// Data for common years:
			0, 3, 3, 6, 1, 4, 6, 2, 5, 0, 3, 5,
			1, 4, 4, 0, 2, 5, 0, 3, 6, 1, 4, 6,
			2, 5, 5, 1, 3, 6, 1, 4, 0, 2, 5, 0,
			3, 6, 6, 2, 4, 0, 2, 5, 1, 3, 6, 1,
			4, 0, 0, 3, 5, 1, 3, 6, 2, 4, 0, 2,
			5, 1, 1, 4, 6, 2, 4, 0, 3, 5, 1, 3,
			6, 2, 2, 5, 0, 3, 5, 1, 4, 6, 2, 4,
			// Data for leap years:
			0, 3, 4, 0, 2, 5, 0, 3, 6, 1, 4, 6,
			1, 4, 5, 1, 3, 6, 1, 4, 0, 2, 5, 0,
			2, 5, 6, 2, 4, 0, 2, 5, 1, 3, 6, 1,
			3, 6, 0, 3, 5, 1, 3, 6, 2, 4, 0, 2,
			4, 0, 1, 4, 6, 2, 4, 0, 3, 5, 1, 3,
			5, 1, 2, 5, 0, 3, 5, 1, 4, 6, 2, 4,
			6, 2, 3, 6, 1, 4, 6, 2, 5, 0, 3, 5,
		];
#endif // #if NET8_0_OR_GREATER && DATEKIT_LOOKUP_TABLES

	// This method is equivalent to DayOfWeek but does not validate its arguments.
	internal static DayOfWeek UncheckedDayOfWeek(Int32 year, Int32 month, Int32 day)
	{
		Debug.Assert(year >= 1);
		Debug.Assert(year <= MaxYear);
		Debug.Assert(month >= January);
		Debug.Assert(month <= December);
		Debug.Assert(day >= 1);
		Debug.Assert(day <= UncheckedDaysInMonth(year, month));

		Int32 sum = day;

#if NET8_0_OR_GREATER && DATEKIT_LOOKUP_TABLES
		sum += DayOfWeekMonthData[s_yearData[year].DayOfWeekMonthDataOffset + month];
#else
		if (month <= February)
		{
			--year;
			sum += 3;
		}

		sum += (month * 81 + 104) >>> 5;
		sum += year + (year >>> 2);
		Int32 century = GetCentury(year);
		sum += -century + (century >>> 2);
#endif
		// Unoptimized:
		//   Int32 dayOfWeek = (sum - 1) % DaysPerWeek;
		// Optimized (valid for sum in [1..0x08000006]):
		const Int32 multiplier = (Int32)((1L << 32) / DaysPerWeek);
		Int32 dayOfWeek = (unchecked(sum * multiplier) >>> 29) - 1;
		// If n = sum - 1 = 7·q + r, where 0 <= r < 7, then sum × multiplier 
		//   = (n + 1)·floor(2^32 / 7)
		//   = (n + 1)·[(2^32 - 4) / 7] because 2^32 mod 7 = 4
		//   = [(2^32)·(7·q + r + 1) - 4·(n + 1)] / 7
		//   = (2^32)·q + [(2^32)·(r + 1) - 4·(n + 1)] / 7.
		// The * operator discards the upper 32 bits (which contain q).
		// The >>> operator divides the lower 32 bits by 2^29, yielding [8·r + 8 - 4·(n + 1) / 2^29] / 7,
		// and then rounds down, yielding floor((8·r + 7) / 7) = r + 1 when n <= 0x08000005.
		return (DayOfWeek)dayOfWeek;
	}

	/// <summary>
	/// Returns the number of days in a specified month and year.
	/// </summary>
	/// <param name="year">
	/// An integer between <see cref="MinYear" /> and <see cref="MaxYear" /> that specifies a year.
	/// </param>
	/// <param name="month">
	/// An integer between 1 and 12 that specifies a month.
	/// </param>
	/// <returns>
	/// The number of days in the specified <paramref name="month" /> of the <paramref name="year" />.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <para>
	/// <paramref name="year" /> is less than <see cref="MinYear" /> or greater than <see cref="MaxYear" />.
	/// </para>
	/// <para>-or-</para>
	/// <para>
	/// <paramref name="month" /> is less than 1 or greater than 12.
	/// </para>
	/// </exception>
	/// <remarks>
	/// This method is equivalent to <see cref="DateTime.DaysInMonth">DateTime.DaysInMonth</see>.
	/// </remarks>
	public static Int32 DaysInMonth(Int32 year, Int32 month)
	{
		ThrowHelper.ThrowIfYearArgumentIsOutOfRange(year, ExceptionArgument.year);
		ThrowHelper.ThrowIfMonthArgumentIsOutOfRange(month, ExceptionArgument.month);
		return UncheckedDaysInMonth(year, month);
	}

	// This method is equivalent to DaysInMonth but does not validate its arguments.
	internal static Int32 UncheckedDaysInMonth(Int32 year, Int32 month)
	{
		Debug.Assert(year >= 1);
		Debug.Assert(month >= January);
		Debug.Assert(month <= December);

		return month != February
			? (month | 30) ^ (month >>> 3)
			: UncheckedIsLeapYear(year) ? LeapDay : DaysInFebruary;

		// (month | 0b11110) equals 30 for even months and 31 for odd months, which is correct if month <= 7
		// but incorrect if month >= 8. In the former case, (month >>> 3) equals 0, and the XOR has no effect.
		// In the latter case, (month >>> 3) equals 1, and the XOR changes 30 to 31 and vice versa.
	}

	// Determines whether a specified month has fewer than 31 days.
	private static Boolean IsShortMonth(Int32 month)
	{
		Debug.Assert(month >= January);
		Debug.Assert(month <= December);

		return (0b010_100_101_010_0 & (1 << month)) != 0;
	}

	// Returns the number of days between March 1 and the first day of a specified month.
	// January and February are specified as months 13 and 14 instead of 1 and 2.
	private static Int32 GetDaysInPreviousMonths(Int32 month)
	{
		Debug.Assert(month >= 3);
		Debug.Assert(month <= 14);

		// Map (3, 4, 5, ..., 12, 13, 14) to (0, 31, 61, ..., 275, 306, 337):
		return (month * 979 - 2919) >>> 5;
	}

	// Returns the number of days between January 1, 0001, and March 1 of a specified year, minus one.
	private static Int32 GetDaysInPreviousYears(Int32 year)
	{
		Debug.Assert(year >= 0);

#if NET8_0_OR_GREATER && DATEKIT_LOOKUP_TABLES
		return s_yearData[year].DaysInPreviousYears;
#else
		// Calculate the number of days between March 1, 0000, and March 1 of the specified year:
		Int32 century = GetCentury(year);
		Int32 daysInPreviousYears = ((year * DaysPer4Years) >>> 2) - century + (century >>> 2);

		// Move the epoch from March 1, 0000, to January 1, 0001, and subtract one:
		return daysInPreviousYears - (DaysPerYear - DaysInJanuary - DaysInFebruary + 1);
#endif
	}

	private static void GetMonthAndDayFromDayOfRotatedYear(Int32 daysSinceMarch1, out Int32 month, out Int32 day)
	{
		Debug.Assert(daysSinceMarch1 >= 0);
		Debug.Assert(daysSinceMarch1 <= DaysPerYear);

		// Unoptimized:
		//   Int32 n3 = daysSinceMarch1 * 5 + 461;
		//   month = n3 / 153;
		//   day = n3 % 153 / 5 + 1;
		// Optimized (valid for daysSinceMarch1 in [0..733]):
		const Int32 shift3 = 16;
		const Int32 multiplier3 = (1 << shift3) * 5 / 153;
		Int32 n3 = daysSinceMarch1 * multiplier3 + 197913;
		month = n3 >>> shift3; // [3..14]
		day = (Int32)((UInt32)n3 % (1 << shift3) / multiplier3) + 1; // [1..31]
	}

	/// <summary>
	/// Determines whether a specified year is a leap year.
	/// </summary>
	/// <param name="year">
	/// An integer between <see cref="MinYear" /> and <see cref="MaxYear" /> that specifies a year.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="year" /> is a leap year in the proleptic Gregorian calendar;
	/// otherwise, <see langword="false" />.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="year" /> is less than <see cref="MinYear" /> or greater than <see cref="MaxYear" />.
	/// </exception>
	/// <remarks>
	/// This method is equivalent to <see cref="DateTime.IsLeapYear">DateTime.IsLeapYear</see>.
	/// </remarks>
	public static Boolean IsLeapYear(Int32 year)
	{
		ThrowHelper.ThrowIfYearArgumentIsOutOfRange(year, ExceptionArgument.year);
		return UncheckedIsLeapYear(year);
	}

	// This method is equivalent to IsLeapYear but does not validate its arguments.
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Boolean UncheckedIsLeapYear(Int32 year)
	{
		Debug.Assert(year >= 1);

		UInt32 unsignedYear = (UInt32)year;

		// If the year is not divisible by 4, it is not a leap year:
		if (unsignedYear % 4 != 0)
			return false;

		// If the year is divisible by 16 (including 400 but excluding 100), it is a leap year:
		if (unsignedYear % 16 == 0)
			return true;

		// If the year is divisible by both 4 and 25, it is divisible by 100 and thus is not a leap year.
		// Otherwise, it is a leap year:
		const Int32 divisor = 25;
		// Unoptimized:
		//   return unsignedYear % divisor != 0;
		// Optimized (valid for year in [0..0x3FFFFFFF]):
		const Int32 multiplier = (Int32)((1L << 32) / divisor + 1);
		return unchecked(unsignedYear * multiplier) >= multiplier;
	}

	// Divides a specified year by 100 and returns the quotient.
	internal static Int32 GetCentury(Int32 year)
	{
		Debug.Assert(year >= 0);

		// Unoptimized:
		//   return year / YearsPerCentury;
		// Optimized (valid for year in [0..43698]; 19 is minimum shift count that encompasses [0..9999]):
		const Int32 shift = 19;
		const Int32 multiplier = (1 << shift) / YearsPerCentury + 1;
		return (year * multiplier) >>> shift;
	}

	// Divides a specified year by 100 and returns the remainder.
	internal static Int32 GetYearOfCentury(Int32 year)
	{
		Debug.Assert(year >= 0);

		// Unoptimized:
		//   return year % YearsPerCentury;
		// Optimized (valid for year in [0..43698]; 19 is minimum shift count that encompasses [0..9999]):
		const Int32 shift = 19;
		const Int32 multiplier = (1 << shift) / YearsPerCentury + 1;
		return (Int32)(((UInt32)year * multiplier % (1 << shift) * YearsPerCentury) >>> shift);
	}
}
