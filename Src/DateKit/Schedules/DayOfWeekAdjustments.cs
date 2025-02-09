using System;
using System.Linq;

namespace DateKit.Schedules;

/// <summary>
/// Represents the adjustments to make to the <see cref="AnnualDayOfMonthRecurrence.Day" />
/// of an <see cref="AnnualDayOfMonthRecurrence" /> when it falls on particular days of the week.
/// </summary>
public class DayOfWeekAdjustments
{
	internal static readonly SByte[] EmptyArray = new SByte[Date.DaysPerWeek];

	private SByte[] _array = new SByte[Date.DaysPerWeek];
	private Boolean _copyOnWrite;

	/// <summary>
	/// Initializes a new instance of the <see cref="DayOfWeekAdjustments" /> class.
	/// </summary>
	public DayOfWeekAdjustments()
	{
	}

	internal DayOfWeekAdjustments(SByte[] array)
	{
		_array = array;
		_copyOnWrite = true;
	}

	/// <summary>
	/// Gets or sets the number of days to add to the <see cref="AnnualDayOfMonthRecurrence.Day" />
	/// of an <see cref="AnnualDayOfMonthRecurrence" /> when it falls on a specified day of the week.
	/// </summary>
	/// <param name="dayOfWeek">
	/// A <see cref="DayOfWeek" />.
	/// </param>
	/// <value>
	/// An integer between -6 and 6 that specifies the number of days to add or subtract. The default is zero.
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <para>
	/// <paramref name="dayOfWeek" /> is less than <see cref="DayOfWeek.Sunday" /> or greater than
	/// <see cref="DayOfWeek.Saturday" />.
	/// </para>
	/// <para>-or-</para>
	/// <para>
	/// The value specified when setting the property is less than -6 or greater than 6.
	/// </para>
	/// </exception>
	public Int32 this[DayOfWeek dayOfWeek]
	{
		get
		{
			SByte[] array = _array;
			Int32 index = (Int32)dayOfWeek;
			if (unchecked((UInt32)index) >= array.Length)
				ThrowHelper.ThrowArgumentOutOfRangeException(dayOfWeek, ExceptionArgument.dayOfWeek);
			return array[index];
		}
		set
		{
			SByte[] array = _array;

			if (_copyOnWrite)
			{
				_array = array = array.ToArray();
				_copyOnWrite = false;
			}

			Int32 index = (Int32)dayOfWeek;
			if (unchecked((UInt32)index) >= array.Length)
				ThrowHelper.ThrowArgumentOutOfRangeException(dayOfWeek, ExceptionArgument.dayOfWeek);
			if (value <= -Date.DaysPerWeek || value >= Date.DaysPerWeek)
				ThrowHelper.ThrowArgumentOutOfRangeException(value, ExceptionArgument.value);
			array[index] = (SByte)value;
		}
	}

	internal SByte[] ToArray()
	{
		_copyOnWrite = true;
		return _array;
	}
}
