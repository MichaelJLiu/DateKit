using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DateKit;

/// <summary>
/// Represents a date between January 1, 0001, and December 31, 9999, in the Common Era (Anno Domini)
/// of the proleptic Gregorian calendar.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
[TypeConverter(typeof(DateConverter))]
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
public readonly partial struct Date : IComparable, IComparable<Date>, IEquatable<Date>, IFormattable
{
	#region Constants

	/// <summary>
	/// Gets the empty <see cref="Date" />.
	/// </summary>
	/// <value>
	/// The empty <see cref="Date" />, equivalent to <c>default(Date)</c>.
	/// </value>
	/// <remarks>
	/// <para>
	/// The <c>default(Date)</c> instance represents the "empty date" instead of an actual date, and can be used as
	/// a sentinel value to indicate the absence of a date. The empty date behaves as follows:
	/// </para>
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// The <see cref="Year" />, <see cref="Month" />, and <see cref="Day" /> properties return 0.
	/// (These properties never return 0 for an actual date.)
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// The empty date compares equal to itself, greater than <see langword="null" />,
	/// and less than <see cref="MinValue" />. (The last behavior differs from that of <see cref="DateTime" />,
	/// whose default instance equals <see cref="DateTime.MinValue" />, or January 1, 0001.)
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// The <see cref="ToIsoString" /> and <see cref="ToString()" /> methods return the empty string.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Except for <see cref="GetHashCode" />, all other properties and methods throw
	/// <see cref="InvalidOperationException" />.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	public static Date Empty => default;

	/// <summary>
	/// Represents the earliest possible <see cref="Year" /> of a <see cref="Date" />.
	/// </summary>
	/// <remarks>
	/// The value of this constant is 1.
	/// </remarks>
	public const Int32 MinYear = 1;

	/// <summary>
	/// Represents the latest possible <see cref="Year" /> of a <see cref="Date" />.
	/// </summary>
	/// <remarks>
	/// The value of this constant is 9999.
	/// </remarks>
	public const Int32 MaxYear = 9999;

	/// <summary>
	/// Gets the earliest possible <see cref="Date" />.
	/// </summary>
	/// <value>
	/// The <see cref="Date" /> that represents January 1, 0001.
	/// </value>
	public static Date MinValue => UnsafeCreate(MinYear, January, 1);

	/// <summary>
	/// Gets the latest possible <see cref="Date" />.
	/// </summary>
	/// <value>
	/// The <see cref="Date" /> that represents December 31, 9999.
	/// </value>
	public static Date MaxValue => UnsafeCreate(MaxYear, December, 31);

	private const Int32 MinDayNumber = 0; // MinValue.DayNumber
	private const Int32 MaxDayNumber = 3652058; // MaxValue.DayNumber

	#endregion Constants

	#region Fields

	[FieldOffset(0)] private readonly Int32 _packedValue;
	[FieldOffset(2)] private readonly UInt16 _year;
	[FieldOffset(1)] private readonly Byte _month;
	[FieldOffset(0)] private readonly Byte _day;
	// The field offsets depend on the architecture:
	//
	// Architecture  |  0  |  1  |  2  |  3  |
	// ------------- | --- | --- | --- | --- |
	// Big-endian    | Year      | Mth | Day |
	// Little-endian | Day | Mth | Year      |

	#endregion Fields

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="Date" /> structure to a specified year, month, and day.
	/// </summary>
	/// <param name="year">
	/// An integer between <see cref="MinYear" /> and <see cref="MaxYear" /> that specifies the year
	/// of the new <see cref="Date" />.
	/// </param>
	/// <param name="month">
	/// An integer between 1 and 12 that specifies the month of the new <see cref="Date" />.
	/// </param>
	/// <param name="day">
	/// An integer between 1 and 31 that specifies the day of the new <see cref="Date" />.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <para>
	/// <paramref name="year" /> is less than <see cref="MinYear" /> or greater than <see cref="MaxYear" />.
	/// </para>
	/// <para>-or-</para>
	/// <para>
	/// <paramref name="month" /> is less than 1 or greater than 12.
	/// </para>
	/// <para>-or-</para>
	/// <para>
	/// <paramref name="day" /> is less than 1 or greater than the number of days
	/// in the specified <paramref name="month" /> of the <paramref name="year" />.
	/// </para>
	/// </exception>
	public Date(Int32 year, Int32 month, Int32 day)
		: this()
	{
		ValidateYear(year, nameof(year));
		ValidateMonth(month, nameof(month));
		ValidateDay(year, month, day, nameof(day));

		// Unoptimized:
		//   _year = (UInt16)year;
		//   _month = (Byte)month;
		//   _day = (Byte)day;
		// Optimized:
		_packedValue = year << 16 | month << 8 | day;
	}

	// This method is equivalent to Date(Int32, Int32, Int32) but does not validate its arguments.
	internal static Date UnsafeCreate(Int32 year, Int32 month, Int32 day)
	{
		Debug.Assert(year >= MinYear);
		Debug.Assert(year <= MaxYear);
		Debug.Assert(month >= January);
		Debug.Assert(month <= December);
		Debug.Assert(day >= 1);
		Debug.Assert(day <= UnsafeDaysInMonth(year, month));

		return new Date(year << 16 | month << 8 | day);
	}

	private Date(Int32 packedValue)
		: this()
	{
		_packedValue = packedValue;

		Debug.Assert(_year >= MinYear);
		Debug.Assert(_year <= MaxYear);
		Debug.Assert(_month >= January);
		Debug.Assert(_month <= December);
		Debug.Assert(_day >= 1);
		Debug.Assert(_day <= UnsafeDaysInMonth(_year, _month));
	}

	/// <summary>
	/// Deconstructs the date into its year, month, and day components.
	/// </summary>
	/// <param name="year">
	/// When this method returns, contains the <see cref="Year" /> of the date.
	/// </param>
	/// <param name="month">
	/// When this method returns, contains the <see cref="Month" /> of the date.
	/// </param>
	/// <param name="day">
	/// When this method returns, contains the <see cref="Day" /> of the date.
	/// </param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Deconstruct(out Int32 year, out Int32 month, out Int32 day)
	{
		year = _year;
		month = _month;
		day = _day;
	}

	#endregion Constructors

	#region Properties

	/// <summary>
	/// Gets the year component of the date.
	/// </summary>
	/// <value>
	/// An integer between <see cref="MinYear" /> and <see cref="MaxYear" /> that specifies the year of the date,
	/// or zero if this instance is <see cref="Empty" />.
	/// </value>
	public Int32 Year => _year;

	/// <summary>
	/// Gets the month component of the date.
	/// </summary>
	/// <value>
	/// An integer between 1 and 12 that specifies the month of the date,
	/// or zero if this instance is <see cref="Empty" />.
	/// </value>
	public Int32 Month => _month;

	/// <summary>
	/// Gets the day component of the date.
	/// </summary>
	/// <value>
	/// An integer between 1 and 31 that specifies the day of the month of the date,
	/// or zero if this instance is <see cref="Empty" />.
	/// </value>
	public Int32 Day => _day;

	/// <summary>
	/// Gets the day number of the date.
	/// </summary>
	/// <value>
	/// An integer between 0 and 3652058 that specifies the number of days between January 1, 0001, and this date.
	/// </value>
	/// <exception cref="InvalidOperationException">
	/// This instance is <see cref="Empty" />.
	/// </exception>
	public Int32 DayNumber
	{
		get
		{
			Int32 year = _year;
			if (year == 0)
				ThrowInvalidOperationException();
			Int32 month = _month;

			// Move January and February to the end of the previous year as months 13 and 14:
			if (month <= February)
			{
				--year;
				month += MonthsPerYear;
			}

			return GetDaysInPreviousYears(year) + GetDaysInPreviousMonths(month) + _day;
		}
	}

	/// <summary>
	/// Gets the day of the week of the date.
	/// </summary>
	/// <value>
	/// The <see cref="System.DayOfWeek" /> that specifies the day of the week of the date.
	/// </value>
	/// <exception cref="InvalidOperationException">
	/// This instance is <see cref="Empty" />.
	/// </exception>
	public DayOfWeek DayOfWeek
	{
		get
		{
			Int32 year = _year;
			if (year == 0)
				ThrowInvalidOperationException();
			return UnsafeDayOfWeek(year, _month, _day);
		}
	}

	/// <summary>
	/// Gets the day of the year of the date.
	/// </summary>
	/// <value>
	/// An integer between 1 and 366 that specifies the day of the year of the date.
	/// </value>
	/// <exception cref="InvalidOperationException">
	/// This instance is <see cref="Empty" />.
	/// </exception>
	public Int32 DayOfYear
	{
		get
		{
			Int32 month = _month;
			if (month == 0)
				ThrowInvalidOperationException();
			Int32 daysInPreviousMonths;

			if (month <= February)
			{
				// Map (1, 2) to (0, 31):
				daysInPreviousMonths = (1 - month) & 31;
			}
			else
			{
				// Map (3, 4, ..., 12) to (59, 90, ..., 334):
				daysInPreviousMonths = (month * 979 - 1030) >>> 5;
				if (UnsafeIsLeapYear(_year))
					++daysInPreviousMonths; // Insert leap day.
			}

			return daysInPreviousMonths + _day;
		}
	}

	private String DebuggerDisplay => $"{{{_year:D4}-{_month:D2}-{_day:D2}}}";

	#endregion Properties

	#region Validation Methods

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void ValidateYear(Int32 year, String paramName)
	{
		// Unoptimized:
		//   if (year < MinYear || year > MaxYear)
		// Optimized:
		if (unchecked((UInt32)(year - MinYear)) > MaxYear - MinYear)
			ThrowArgumentOutOfRangeException(paramName);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void ValidateMonth(Int32 month, String paramName)
	{
		// Unoptimized:
		//   if (month < January || month > December)
		// Optimized:
		if (unchecked((UInt32)(month - January)) > December - January)
			ThrowArgumentOutOfRangeException(paramName);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void ValidateDay(Int32 year, Int32 month, Int32 day, String paramName)
	{
		// Unoptimized:
		//   if (day < 1 || day > UnsafeDaysInMonth(year, month))
		// Optimized:
		if (unchecked((UInt32)(day - 1)) >= UnsafeDaysInMonth(year, month))
			ThrowArgumentOutOfRangeException(paramName);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static UInt32 ValidateDayNumber(Int32 dayNumber, String paramName)
	{
		// Unoptimized:
		//   if (dayNumber < MinDayNumber || dayNumber > MaxDayNumber)
		// Optimized:
		if (unchecked((UInt32)(dayNumber - MinDayNumber)) > MaxDayNumber - MinDayNumber)
			ThrowArgumentOutOfRangeException(paramName);
		return (UInt32)dayNumber;
	}

	private static void ThrowArgumentOutOfRangeException(String paramName)
	{
		throw new ArgumentOutOfRangeException(paramName);
	}

	private static void ThrowInvalidOperationException()
	{
		throw new InvalidOperationException("Operation is not supported by the default (empty) Date.");
	}

	#endregion Validation Methods
}
