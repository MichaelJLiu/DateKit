using System;
using System.Diagnostics;
#if NET7_0_OR_GREATER // for StringSyntaxAttribute
using System.Diagnostics.CodeAnalysis;
#endif

namespace DateKit;

partial struct Date : IFormattable
{
#if NET6_0_OR_GREATER
	/// <summary>
	/// Converts a specified <see cref="DateOnly" /> to the equivalent <see cref="Date" />.
	/// </summary>
	/// <param name="dateOnly">
	/// The <see cref="DateOnly" /> to convert.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> that represents the same date as <paramref name="dateOnly" />.
	/// </returns>
	public static Date FromDateOnly(DateOnly dateOnly)
	{
		return UncheckedFromDayNumber(dateOnly.DayNumber);
	}
#endif // #if NET6_0_OR_GREATER

	/// <summary>
	/// Converts the date component of a specified <see cref="DateTime" /> to the equivalent <see cref="Date" />.
	/// </summary>
	/// <param name="dateTime">
	/// The <see cref="DateTime" /> to convert. Its time component is ignored.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> that represents the date component of <paramref name="dateTime" />.
	/// </returns>
	public static Date FromDateTime(DateTime dateTime)
	{
		return UncheckedFromDayNumber((Int32)((UInt64)dateTime.Ticks / TimeSpan.TicksPerDay));
	}

	/// <summary>
	/// Converts a specified day number to the equivalent <see cref="Date" />.
	/// </summary>
	/// <param name="dayNumber">
	/// An integer between 0 and 3652058 that specifies a number of days since January 1, 0001.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> whose <see cref="DayNumber" /> equals <paramref name="dayNumber" />.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="dayNumber" /> is less than 0 or greater than 3652058.
	/// </exception>
	public static Date FromDayNumber(Int32 dayNumber)
	{
		// Unoptimized:
		//   if (dayNumber < MinDayNumber || dayNumber > MaxDayNumber)
		// Optimized:
		if (unchecked((UInt32)(dayNumber - MinDayNumber)) > MaxDayNumber - MinDayNumber)
			ThrowHelper.ThrowArgumentOutOfRangeException(dayNumber, ExceptionArgument.dayNumber);
		return UncheckedFromDayNumber(dayNumber);
	}

	// This method is equivalent to FromDayNumber but does not validate its argument.
	// The implementation is based on the paper "Euclidean affine functions and applications to calendar algorithms"
	// by Cassio Neri and Lorenz Schneider (https://arxiv.org/pdf/2102.06959.pdf).
	private static Date UncheckedFromDayNumber(Int32 dayNumber)
	{
		// ReSharper disable once ConditionIsAlwaysTrueOrFalse
		Debug.Assert(dayNumber >= MinDayNumber);
		Debug.Assert(dayNumber <= MaxDayNumber);

		// Move the epoch from January 1, 0001, to March 1, 0000:
		dayNumber += DaysPerYear - DaysInJanuary - DaysInFebruary;

		Int32 n1 = dayNumber * 4 + 3;
		Int32 century = (Int32)((UInt32)n1 / DaysPer400Years);
		Int32 n2 = (n1 - century * DaysPer400Years) | 3; // day of century * 4 + 3

		// Unoptimized:
		//   Int32 yearOfCentury = n2 / DaysPer4Years;
		//   Int32 daysSinceMarch1 = n2 % DaysPer4Years / 4;
		// Optimized:
		const Int32 multiplier2 = (Int32)((1L << 32) / DaysPer4Years) + 1;
		Int64 u2 = (Int64)(UInt32)n2 * multiplier2;
		Int32 yearOfCentury = (Int32)(u2 >>> 32);
		Int32 daysSinceMarch1 = (Int32)(unchecked((UInt32)u2) / (multiplier2 * 4));

		Int32 year = century * YearsPerCentury + yearOfCentury;

		// Unoptimized:
		//   Int32 n3 = daysSinceMarch1 * 5 + 461;
		//   Int32 month = n3 / 153;
		//   Int32 day = n3 % 153 / 5 + 1;
		// Optimized (valid for daysSinceMarch1 in [0..733]):
		const Int32 shift3 = 16;
		const Int32 multiplier3 = (1 << shift3) * 5 / 153;
		Int32 n3 = daysSinceMarch1 * multiplier3 + 197913;
		Int32 month = n3 >>> shift3; // [3..14]
		Int32 day = (Int32)((UInt32)n3 % (1 << shift3) / multiplier3) + 1; // [1..31]

		// Move January and February to the beginning of the next year:
		if (month > December)
		{
			++year;
			month -= MonthsPerYear;
		}

		return UncheckedCreate(year, month, day);
	}

	/// <summary>
	/// Converts a specified ISO 8601 string representation of a date to its equivalent <see cref="Date" />.
	/// </summary>
	/// <param name="s">
	/// The string representation of a date in the format "yyyy-MM-dd".
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> that is equivalent to <paramref name="s" />.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="s" /> is <see langword="null" />.
	/// </exception>
	/// <exception cref="FormatException">
	/// <paramref name="s" /> does not contain a valid ISO 8601 string representation of a date.
	/// </exception>
	public static Date ParseIsoString(String s)
	{
		return DatePattern.Iso8601.ParseExact(s);
	}

#if NET6_0_OR_GREATER
	/// <summary>
	/// Converts the <see cref="Date" /> to the equivalent <see cref="DateOnly" />.
	/// </summary>
	/// <returns>
	/// The <see cref="DateOnly" /> that represents the same date as this instance.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// This instance is <see cref="Empty" />.
	/// </exception>
	public DateOnly ToDateOnly()
	{
		return DateOnly.FromDayNumber(this.DayNumber);
	}
#endif // #if NET6_0_OR_GREATER

	/// <summary>
	/// Converts the <see cref="Date" /> to the equivalent <see cref="DateTime" />.
	/// </summary>
	/// <param name="kind">
	/// The <see cref="DateTime.Kind" /> of the resulting <see cref="DateTime" />.
	/// The default is <see cref="DateTimeKind.Unspecified" />.
	/// </param>
	/// <returns>
	/// The <see cref="DateTime" /> that represents the same date as this instance
	/// and that has a time component of midnight (00:00:00).
	/// </returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="kind" /> is not one of the <see cref="DateTimeKind" /> values.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// This instance is <see cref="Empty" />.
	/// </exception>
	public DateTime ToDateTime(DateTimeKind kind = DateTimeKind.Unspecified)
	{
		return new DateTime(this.DayNumber * TimeSpan.TicksPerDay, kind);
	}

	/// <summary>
	/// Converts the date to its equivalent ISO 8601 string representation.
	/// </summary>
	/// <returns>
	/// The string representation of the date in the format "yyyy-MM-dd",
	/// or the empty string if this instance is <see cref="Empty" />.
	/// </returns>
	public String ToIsoString()
	{
		return DatePattern.Iso8601.Format(this);
	}

	/// <overloads>
	/// Converts the date to its equivalent string representation.
	/// </overloads>
	/// <summary>
	/// Converts the date to its equivalent string representation using the standard format specifier "d"
	/// and the formatting conventions of the current culture.
	/// </summary>
	/// <returns>
	/// The string representation of the date,
	/// or the empty string if this instance is <see cref="Empty" />.
	/// </returns>
	public override String ToString()
	{
#pragma warning disable CA1305 // Specify IFormatProvider
		return this.ToString(format: null);
#pragma warning restore CA1305
	}

	/// <summary>
	/// Converts the date to its equivalent string representation using a specified format string
	/// and culture-specific format information.
	/// </summary>
	/// <param name="format">
	/// A date format string,
	/// or either <see langword="null" /> or the empty string to use the standard format specifier "d".
	/// </param>
	/// <param name="provider">
	/// An <see cref="IFormatProvider" /> that supplies culture-specific format information,
	/// or <see langword="null" /> to use the current culture.
	/// </param>
	/// <returns>
	/// The string representation of the date as specified by <paramref name="format" /> and
	/// <paramref name="provider" />, or the empty string if this instance is <see cref="Empty" />.
	/// </returns>
	/// <exception cref="FormatException">
	/// <paramref name="format" /> is not a valid date format string.
	/// </exception>
	/// <remarks>
	/// See <see cref="DatePattern.Create" /> for a list of the standard and custom format specifiers
	/// supported by this method.
	/// </remarks>
	public String ToString(
#if NET7_0_OR_GREATER
		[StringSyntax(StringSyntaxAttribute.DateOnlyFormat)]
#endif
		String? format,
#pragma warning disable CA1725 // Parameter names should match base declaration (formatProvider)
		IFormatProvider? provider = null)
#pragma warning restore CA1725
	{
		return DatePattern.Create(format, provider).Format(this);
	}
}
