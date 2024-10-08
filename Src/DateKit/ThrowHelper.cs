using System;
using System.Runtime.CompilerServices;

namespace DateKit;

internal enum ExceptionArgument
{
	// ReSharper disable InconsistentNaming
	value,
	year,
	month,
	day,
	dayOfWeek,
	date,
	date1,
	date2,
	dayNumber,
	// ReSharper enable InconsistentNaming
}

internal static class ThrowHelper
{
	private static readonly String[] s_paramNames =
	[
		nameof(ExceptionArgument.value),
		nameof(ExceptionArgument.year),
		nameof(ExceptionArgument.month),
		nameof(ExceptionArgument.day),
		nameof(ExceptionArgument.dayOfWeek),
		nameof(ExceptionArgument.date),
		nameof(ExceptionArgument.date1),
		nameof(ExceptionArgument.date2),
		nameof(ExceptionArgument.dayNumber),
	];

	public static void ThrowArgumentNullException(String? paramName)
	{
		throw new ArgumentNullException(paramName);
	}

	// Optimization note: This method is generic to avoid boxing the actualValue at call sites.
	public static void ThrowArgumentOutOfRangeException<TValue>(TValue actualValue, ExceptionArgument argument)
	{
		throw new ArgumentOutOfRangeException(GetParamName(argument), actualValue, message: null);
	}

	public static void ThrowEmptyDateArgumentException(ExceptionArgument argument)
	{
		throw new ArgumentException("Operation is not supported by the default (empty) Date.", GetParamName(argument));
	}

	public static void ThrowEmptyDateInvalidOperationException()
	{
		throw new InvalidOperationException("Operation is not supported by the default (empty) Date.");
	}

	public static void ThrowOverflowException()
	{
		throw new OverflowException("The resulting date exceeds the range of the Date type.");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfArgumentIsNull(
		Object? value, [CallerArgumentExpression(nameof(value))] String? paramName = null)
	{
		if (value == null)
			ThrowArgumentNullException(paramName);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfYearArgumentIsOutOfRange(Int32 year, ExceptionArgument argument)
	{
		// Unoptimized:
		//   if (year < Date.MinYear || year > Date.MaxYear)
		// Optimized:
		if (unchecked((UInt32)(year - Date.MinYear)) > Date.MaxYear - Date.MinYear)
			ThrowArgumentOutOfRangeException(year, argument);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfMonthArgumentIsOutOfRange(Int32 month, ExceptionArgument argument)
	{
		// Unoptimized:
		//   if (month < Date.January || month > Date.December)
		// Optimized:
		if (unchecked((UInt32)(month - Date.January)) > Date.December - Date.January)
			ThrowArgumentOutOfRangeException(month, argument);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfDayArgumentIsOutOfRange(Int32 month, Int32 day, ExceptionArgument argument)
	{
		// Unoptimized:
		//   if (day < 1 || (day > Date.MinDaysPerMonth && day > Date.UnsafeDaysInMonth(month)))
		// Optimized:
		UInt32 dayMinusOne = unchecked((UInt32)(day - 1));
		if (dayMinusOne >= Date.MinDaysPerMonth && dayMinusOne >= Date.UnsafeDaysInMonth(month))
			ThrowArgumentOutOfRangeException(day, argument);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfDayArgumentIsOutOfRange(Int32 year, Int32 month, Int32 day, ExceptionArgument argument)
	{
		// Unoptimized:
		//   if (day < 1 || (day > Date.MinDaysPerMonth && day > Date.UnsafeDaysInMonth(year, month)))
		// Optimized:
		UInt32 dayMinusOne = unchecked((UInt32)(day - 1));
		if (dayMinusOne >= Date.MinDaysPerMonth && dayMinusOne >= Date.UnsafeDaysInMonth(year, month))
			ThrowArgumentOutOfRangeException(day, argument);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfDateArgumentIsEmpty(Date date, ExceptionArgument argument)
	{
		if (date == Date.Empty)
			ThrowEmptyDateArgumentException(argument);
	}

	private static String GetParamName(ExceptionArgument argument)
	{
		return s_paramNames[(Int32)argument];
	}
}
