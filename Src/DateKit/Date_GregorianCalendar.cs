using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DateKit;

partial struct Date
{
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
		// of this year, minus 1 modulo DaysPerWeek to compensate for adding 1-based day values,
		// plus DaysPerWeek if this is a leap year, multiplied by MonthsPerYear.
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
		Int32 daysInPreviousYears = -(DaysPerYear - 31 - 28 + 1); // January 1, 0001, to March 1, 0000, minus one
		Int32 firstDayOfWeek = 0; // Monday, January 1, 0001, minus one
		data[0].DaysInPreviousYears = daysInPreviousYears;

		for (Int32 year = 1; year <= MaxYear; ++year)
		{
			Boolean isLeapYear = UnsafeIsLeapYear(year);

			daysInPreviousYears += DaysPerYear + (isLeapYear ? 1 : 0);
			data[year].DaysInPreviousYears = daysInPreviousYears;
			data[year].DayOfWeekMonthDataOffset = (Byte)((firstDayOfWeek + (isLeapYear ? DaysPerWeek : 0)) * 12);

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
#endif

	// This method is equivalent to DayOfWeek but does not validate its arguments.
	internal static DayOfWeek UnsafeDayOfWeek(Int32 year, Int32 month, Int32 day)
	{
		Debug.Assert(year >= 1);
		Debug.Assert(year <= MaxYear);
		Debug.Assert(month >= January);
		Debug.Assert(month <= December);
		Debug.Assert(day >= 1);
		Debug.Assert(day <= UnsafeDaysInMonth(year, month));

		Int32 sum = day;

#if NET8_0_OR_GREATER && DATEKIT_LOOKUP_TABLES
		sum += DayOfWeekMonthData[s_yearData[year].DayOfWeekMonthDataOffset + month];
		// Unoptimized:
		//   Int32 dayOfWeek = sum % DaysPerWeek;
		// Optimized (valid for sum in [0, 85]; 8 is minimum shift count that encompasses [1, 6 + 31]):
		const Int32 shift = 8;
		const Int32 multiplier = (1 << shift) / DaysPerWeek + 1;
#else
		if (month <= February)
		{
			--year;
			sum += 3;
		}

		sum += (month * 81 + 72) >>> 5;
		sum += year + (year >>> 2);
		Int32 century = GetCentury(year);
		sum += -century + (century >>> 2);
		// Unoptimized:
		//   Int32 dayOfWeek = sum % DaysPerWeek;
		// Optimized (valid for sum in [0, 13107]):
		const Int32 shift = 16;
		const Int32 multiplier = (1 << shift) / DaysPerWeek + 1;
#endif
		Int32 dayOfWeek = (Int32)((UInt32)sum * multiplier % (1 << shift) * DaysPerWeek >>> shift);
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
		ValidateYear(year, nameof(year));
		ValidateMonth(month, nameof(month));
		return UnsafeDaysInMonth(year, month);
	}

	// This method is equivalent to DaysInMonth but does not validate its arguments.
	internal static Int32 UnsafeDaysInMonth(Int32 year, Int32 month)
	{
		Debug.Assert(year >= 1);
		Debug.Assert(month >= January);
		Debug.Assert(month <= December);

		return month != February
			? ((month >>> 3) ^ month) | 30
			: UnsafeIsLeapYear(year) ? 29 : 28;

		// month               | (month >> 3) ^ month | ... OR 0b1110
		// ------------------- | -------------------- | -------------
		// 1, 3, 5, 7 = 0b0XX1 | 0 ^ 0b0XX1 = 0b0XX1  | 0b1111 = 31
		// 4, 6       = 0b0XX0 | 0 ^ 0b0XX0 = 0b0XX0  | 0b1110 = 30
		// 8, 10, 12  = 0b1XX0 | 1 ^ 0b1XX0 = 0b1XX1  | 0b1111 = 31
		// 9, 11      = 0b1XX1 | 1 ^ 0b1XX1 = 0b1XX0  | 0b1110 = 30
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
		Int32 daysInPreviousYears = (year * DaysPer4Years >>> 2) - century + (century >>> 2);

		// Move the epoch from March 1, 0000, to January 1, 0001, and subtract one:
		return daysInPreviousYears - (DaysPerYear - 31 - 28 + 1);
#endif
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
		ValidateYear(year, nameof(year));
		return UnsafeIsLeapYear(year);
	}

	// This method is equivalent to IsLeapYear but does not validate its arguments.
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Boolean UnsafeIsLeapYear(Int32 year)
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
		// Optimized (valid for year in [0, 43690]; 17 is minimum shift count that encompasses [0, 9999]):
		const Int32 shift = 17;
		const Int32 multiplier = (1 << shift) / divisor + 1;
		return unsignedYear * multiplier % (1 << shift) >= multiplier;
	}

	// Divides a specified year by 100 and returns the quotient.
	internal static Int32 GetCentury(Int32 year)
	{
		Debug.Assert(year >= 0);

		const Int32 divisor = 100;
		// Unoptimized:
		//   return year / divisor;
		// Optimized (valid for year in [0, 43698]; 19 is minimum shift count that encompasses [0, 9999]):
		const Int32 shift = 19;
		const Int32 multiplier = (1 << shift) / divisor + 1;
		return year * multiplier >>> shift;
	}

	// Divides a specified year by 100 and returns the remainder.
	internal static Int32 GetYearOfCentury(Int32 year)
	{
		Debug.Assert(year >= 0);

		const Int32 divisor = 100;
		// Unoptimized:
		//   return year % divisor;
		// Optimized (valid for year in [0, 43698]; 19 is minimum shift count that encompasses [0, 9999]):
		const Int32 shift = 19;
		const Int32 multiplier = (1 << shift) / divisor + 1;
		return (Int32)((UInt32)year * multiplier % (1 << shift) * divisor >>> shift);
	}
}
