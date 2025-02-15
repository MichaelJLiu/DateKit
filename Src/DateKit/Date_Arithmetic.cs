using System;
using System.Diagnostics;

namespace DateKit;

partial struct Date
{
	/// <summary>
	/// Adds a specified number of years to the date and returns the result.
	/// </summary>
	/// <param name="number">
	/// The number of years to add or, if the value is negative, subtract.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> that results from adding the specified <paramref name="number" /> of years
	/// to this instance.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// This instance is <see cref="Empty" />.
	/// </exception>
	/// <exception cref="OverflowException">
	/// The resulting <see cref="Date" /> is less than <see cref="MinValue" /> or greater than <see cref="MaxValue" />.
	/// </exception>
	/// <remarks>
	/// The month component is unchanged. The day component is unchanged unless this instance represents February 29
	/// in a leap year and the resulting year is a common year. In that case, the resulting day is February 28.
	/// </remarks>
	public Date AddYears(Int32 number)
	{
		Int32 packedValue = _packedValue;
		if (packedValue == 0)
			ThrowHelper.ThrowEmptyDateInvalidOperationException();

		const Int32 maxNumber = MaxYear - MinYear;
		// Unoptimized:
		//   if (number < -maxNumber || number > maxNumber)
		// Optimized:
		if (unchecked((UInt32)(number + maxNumber)) > maxNumber * 2)
			ThrowHelper.ThrowOverflowException();

		packedValue += PackYear(number);
		Int32 year = UnpackYear(packedValue);
		// Unoptimized:
		//   if (year < MinYear || year > MaxYear)
		// Optimized:
		if (unchecked((UInt32)(year - MinYear)) > MaxYear - MinYear)
			ThrowHelper.ThrowOverflowException();
		if (ExtractMonthDay(packedValue) == PackMonthDay(February, LeapDay) && !UncheckedIsLeapYear(year))
			--packedValue;
		return new Date(packedValue);
	}

	/// <summary>
	/// Adds a specified number of months to the date and returns the result.
	/// </summary>
	/// <param name="number">
	/// The number of months to add or, if the value is negative, subtract.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> that results from adding the specified <paramref name="number" /> of months
	/// to this instance.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// This instance is <see cref="Empty" />.
	/// </exception>
	/// <exception cref="OverflowException">
	/// The resulting <see cref="Date" /> is less than <see cref="MinValue" /> or greater than <see cref="MaxValue" />.
	/// </exception>
	/// <remarks>
	/// The day component is unchanged unless it is not a valid day in the resulting month and year.
	/// In that case, the resulting day is the last day of the resulting month and year. For example,
	/// adding one month to January 31 yields February 28 in a common year and February 29 in a leap year.
	/// </remarks>
	public Date AddMonths(Int32 number)
	{
		if (number >= 0)
		{
			if (number <= MonthsPerYear)
				return AddSmallPositiveMonths(this, number);
		}
		else
		{
			if (number >= -MonthsPerYear)
				return AddSmallNegativeMonths(this, number);
		}

		return AddLargeMonths(this, number);
	}

	private static Date AddSmallNegativeMonths(Date date, Int32 number)
	{
		Debug.Assert(number <= 0);
		Debug.Assert(number >= -MonthsPerYear);

		Int32 packedValue = date._packedValue;
		if (packedValue == 0)
			ThrowHelper.ThrowEmptyDateInvalidOperationException();

		packedValue += PackMonth(number);

		if (ExtractSignedMonthDay(packedValue) < PackMonthDay(January, 1))
		{
			if (packedValue >= MinValue._packedValue + PackYear(1) - PackMonth(MonthsPerYear))
				packedValue = packedValue - PackYear(1) + PackMonth(MonthsPerYear);
			else
				ThrowHelper.ThrowOverflowException();
		}

		Int32 day = UnpackDay(packedValue);

		if (day > MinDaysPerMonth)
		{
			Int32 month = UnpackMonth(packedValue);
			if (IsShortMonth(month))
				packedValue = ClampDayOfShortMonth(packedValue, month, day);
		}

		return new Date(packedValue);
	}

	private static Date AddSmallPositiveMonths(Date date, Int32 number)
	{
		Debug.Assert(number >= 0);
		Debug.Assert(number <= MonthsPerYear);

		Int32 packedValue = date._packedValue;
		if (packedValue == 0)
			ThrowHelper.ThrowEmptyDateInvalidOperationException();

		packedValue += PackMonth(number);

		if (ExtractMonthDay(packedValue) > PackMonthDay(December, DaysInDecember))
		{
			if (packedValue <= MaxValue._packedValue - PackYear(1) + PackMonth(MonthsPerYear))
				packedValue = packedValue + PackYear(1) - PackMonth(MonthsPerYear);
			else
				ThrowHelper.ThrowOverflowException();
		}

		Int32 day = UnpackDay(packedValue);

		if (day > MinDaysPerMonth)
		{
			Int32 month = UnpackMonth(packedValue);
			if (IsShortMonth(month))
				packedValue = ClampDayOfShortMonth(packedValue, month, day);
		}

		return new Date(packedValue);
	}

	private static Int32 ClampDayOfShortMonth(Int32 packedValue, Int32 month, Int32 day)
	{
		Debug.Assert(month == UnpackMonth(packedValue));
		Debug.Assert(month >= January);
		Debug.Assert(month <= December);
		Debug.Assert(IsShortMonth(month));
		Debug.Assert(day == UnpackDay(packedValue));
		Debug.Assert(day >= 1);
		Debug.Assert(day <= MaxDaysPerMonth);

		if (month != February)
		{
			if (day == MaxDaysPerMonth)
				--packedValue;
		}
		else
		{
			Int32 year = UnpackYear(packedValue);
			Int32 daysInMonth = UncheckedDaysInMonth(year, month);
			Int32 excessDays = day - daysInMonth;
			if (excessDays > 0)
				packedValue -= excessDays;
		}

		return packedValue;
	}

	private static Date AddLargeMonths(Date date, Int32 number)
	{
		Int32 year = date.Year;
		if (year == 0)
			ThrowHelper.ThrowEmptyDateInvalidOperationException();

		const Int32 maxNumber = (MaxYear - MinYear + 1) * MonthsPerYear - 1;
		// Unoptimized:
		//   if (number < -maxNumber || number > maxNumber)
		// Optimized:
		if (unchecked((UInt32)(number + maxNumber)) > maxNumber * 2)
			ThrowHelper.ThrowOverflowException();

		Int32 month = date.Month + number;

		// Unoptimized:
		//   Int32 offsetYears = month > 0 ? (Int32)(((UInt32)month - 1) / MonthsPerYear) : month / MonthsPerYear - 1;
		// Optimized (valid for month in [-131074..131075]):
		const Int32 shift = 19;
		const Int32 multiplier = (1 << shift) / MonthsPerYear + 1;
		Int32 offsetYears = (Int32)(((Int64)(month >= 0 ? month - 1 : month) * multiplier) >> shift);

		year += offsetYears;
		// Unoptimized:
		//   if (year < MinYear || year > MaxYear)
		// Optimized:
		if (unchecked((UInt32)(year - MinYear)) > MaxYear - MinYear)
			ThrowHelper.ThrowOverflowException();
		month -= offsetYears * MonthsPerYear;
		Int32 day = date.Day;

		if (day > MinDaysPerMonth && IsShortMonth(month))
		{
			Int32 daysInMonth = month != February
				? 30
				: UncheckedDaysInMonth(year, month);
			if (day > daysInMonth)
				day = daysInMonth;
		}

		return UncheckedCreate(year, month, day);
	}

	/// <summary>
	/// Adds a specified number of days to the date and returns the result.
	/// </summary>
	/// <param name="number">
	/// The number of days to add or, if the value is negative, subtract.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> that results from adding the specified <paramref name="number" /> of days
	/// to this instance.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// This instance is <see cref="Empty" />.
	/// </exception>
	/// <exception cref="OverflowException">
	/// The resulting <see cref="Date" /> is less than <see cref="MinValue" /> or greater than <see cref="MaxValue" />.
	/// </exception>
	public Date AddDays(Int32 number)
	{
		if (number >= 0)
		{
			return number <= MinDaysPerMonth
				? AddSmallPositiveDays(this, number)
				: number <= DaysPerYear
					? AddMediumPositiveDays(this, number)
					: AddLargePositiveDays(this, number);
		}
		else
		{
			return number >= -MinDaysPerMonth
				? AddSmallNegativeDays(this, number)
				: number >= -DaysPerYear
					? AddMediumNegativeDays(this, number)
					: AddLargeNegativeDays(this, number);
		}
	}

	private static Date AddSmallNegativeDays(Date date, Int32 number)
	{
		Debug.Assert(number <= 0);
		Debug.Assert(number >= -MinDaysPerMonth);

		Int32 packedValue = date._packedValue;
		if (packedValue == 0)
			ThrowHelper.ThrowEmptyDateInvalidOperationException();

		packedValue += number;
		Int32 day = UnpackSignedDay(packedValue);

		if (day <= 0)
		{
			Int32 year = UnpackYear(packedValue);
			Int32 month = UnpackMonth(packedValue - 1); // subtract one day to ensure month rollover

			if (month >= January)
			{
				packedValue = packedValue
					- PackMonth(1) // decrement month
					+ UncheckedDaysInMonth(year, month);
			}
			else if (year > MinYear)
			{
				packedValue = packedValue
					- PackYear(1) // decrement year
					+ PackMonth(December) - PackMonth(January) // change month from January to December
					+ DaysInDecember;
			}
			else
			{
				ThrowHelper.ThrowOverflowException();
			}
		}

		return new Date(packedValue);
	}

	private static Date AddMediumNegativeDays(Date date, Int32 number)
	{
		Debug.Assert(number <= 0);
		Debug.Assert(number >= -DaysPerYear);

		Int32 year = date.Year;
		if (year == 0)
			ThrowHelper.ThrowEmptyDateInvalidOperationException();
		Int32 month = date.Month;
		Int32 day = date.Day;

		// Move January and February to the end of the previous year as months 13 and 14:
		if (month <= February)
		{
			--year;
			month += MonthsPerYear;
		}

		Int32 daysSinceMarch1 = GetDaysInPreviousMonths(month) + day + number - 1;

		if (daysSinceMarch1 < 0)
		{
			daysSinceMarch1 += DaysPerYear + (UncheckedIsLeapYear(year) ? 1 : 0);
			--year;
		}

		GetMonthAndDayFromDayOfRotatedYear(daysSinceMarch1, out month, out day);

		// Move January and February to the beginning of the next year:
		if (month > December)
		{
			++year;
			month -= MonthsPerYear;
		}

		if (year < MinYear)
			ThrowHelper.ThrowOverflowException();

		return UncheckedCreate(year, month, day);
	}

	private static Date AddLargeNegativeDays(Date date, Int32 number)
	{
		Debug.Assert(number <= 0);

		Int32 dayNumber = date.DayNumber + number; // DayNumber checks for an empty date.
		if (dayNumber < MinDayNumber)
			ThrowHelper.ThrowOverflowException();

		return UncheckedFromDayNumber(dayNumber);
	}

	private static Date AddSmallPositiveDays(Date date, Int32 number)
	{
		Debug.Assert(number >= 0);
		Debug.Assert(number <= MinDaysPerMonth);

		Int32 packedValue = date._packedValue;
		if (packedValue == 0)
			ThrowHelper.ThrowEmptyDateInvalidOperationException();

		packedValue += number;
		Int32 day = UnpackDay(packedValue);

		if (day > MinDaysPerMonth)
		{
			Int32 year = UnpackYear(packedValue);
			Int32 month = UnpackMonth(packedValue);
			Int32 daysInMonth = UncheckedDaysInMonth(year, month);

			if (day > daysInMonth)
			{
				packedValue -= daysInMonth;

				if (month < December)
				{
					packedValue += PackMonth(1); // increment month
				}
				else if (year < MaxYear)
				{
					packedValue = packedValue
						+ PackYear(1) // increment year
						+ PackMonth(January) - PackMonth(December); // change month from December to January
				}
				else
				{
					ThrowHelper.ThrowOverflowException();
				}
			}
		}

		return new Date(packedValue);
	}

	private static Date AddMediumPositiveDays(Date date, Int32 number)
	{
		Debug.Assert(number >= 0);
		Debug.Assert(number <= DaysPerYear);

		Int32 year = date.Year;
		if (year == 0)
			ThrowHelper.ThrowEmptyDateInvalidOperationException();
		Int32 month = date.Month;
		Int32 day = date.Day;

		// Move January and February to the end of the previous year as months 13 and 14:
		if (month <= February)
		{
			--year;
			month += MonthsPerYear;
		}

		Int32 daysSinceMarch1 = GetDaysInPreviousMonths(month) + day + number - 1;
		Int32 daysSinceMarch1OfNextYear = daysSinceMarch1 - DaysPerYear;

		if (daysSinceMarch1OfNextYear >= 0 && (!UncheckedIsLeapYear(year + 1) || --daysSinceMarch1OfNextYear >= 0))
		{
			++year;
			daysSinceMarch1 = daysSinceMarch1OfNextYear;
		}

		GetMonthAndDayFromDayOfRotatedYear(daysSinceMarch1, out month, out day);

		// Move January and February to the beginning of the next year:
		if (month > December)
		{
			++year;
			month -= MonthsPerYear;
		}

		if (year > MaxYear)
			ThrowHelper.ThrowOverflowException();

		return UncheckedCreate(year, month, day);
	}

	private static Date AddLargePositiveDays(Date date, Int32 number)
	{
		Debug.Assert(number >= 0);

		Int32 dayNumber = unchecked(date.DayNumber + number); // DayNumber checks for an empty date.
		if (unchecked((UInt32)dayNumber) > MaxDayNumber)
			ThrowHelper.ThrowOverflowException();

		return UncheckedFromDayNumber(dayNumber);
	}

	/// <summary>
	/// Subtracts one specified <see cref="Date" /> from another, yielding the number of days between them.
	/// </summary>
	/// <param name="date1">
	/// The <see cref="Date" /> from which to subtract.
	/// </param>
	/// <param name="date2">
	/// The <see cref="Date" /> to subtract.
	/// </param>
	/// <returns>
	/// <para>
	/// A signed integer whose magnitude equals the number of days between <paramref name="date1" />
	/// and <paramref name="date2" />, and whose sign indicates their relative order.
	/// </para>
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>Less than zero</term>
	/// <description>
	/// <paramref name="date1" /> is less (earlier) than <paramref name="date2" />.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Zero</term>
	/// <description>
	/// <paramref name="date1" /> is equal to <paramref name="date2" />.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Greater than zero</term>
	/// <description>
	/// <paramref name="date1" /> is greater (later) than <paramref name="date2" />.
	/// </description>
	/// </item>
	/// </list>
	/// </returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="date1" /> or <paramref name="date2" /> is <see cref="Empty" />.
	/// </exception>
	public static Int32 Subtract(Date date1, Date date2)
	{
		Int32 packedValue1 = date1._packedValue;
		if (packedValue1 == 0)
			ThrowHelper.ThrowEmptyDateArgumentException(ExceptionArgument.date1);
		Int32 packedValue2 = date2._packedValue;
		if (packedValue2 == 0)
			ThrowHelper.ThrowEmptyDateArgumentException(ExceptionArgument.date2);

		Int32 result = packedValue1 - packedValue2;

		// Unoptimized:
		//   if (result < -MaxDaysPerMonth || result > MaxDaysPerMonth)
		// Optimized:
		if (unchecked((UInt32)(result + MaxDaysPerMonth)) > MaxDaysPerMonth * 2)
		{
			result = UnpackSignedDay(result);

			Int32 year1 = date1.Year;
			Int32 year2 = date2.Year;
			Int32 month1 = date1.Month;
			Int32 month2 = date2.Month;

			// Move January and February to the end of the previous year as months 13 and 14:
			if (month1 <= February)
			{
				--year1;
				month1 += MonthsPerYear;
			}

			if (month2 <= February)
			{
				--year2;
				month2 += MonthsPerYear;
			}

			if (month1 != month2)
				result += GetDaysInPreviousMonths(month1) - GetDaysInPreviousMonths(month2);

			if (year1 != year2)
				result += GetDaysInPreviousYears(year1) - GetDaysInPreviousYears(year2);
		}

		return result;
	}

	/// <summary>
	/// Decrements a specified date by one day.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> to decrement.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> that represents the day before the specified <paramref name="date" />.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// <paramref name="date" /> is <see cref="Empty" />.
	/// </exception>
	/// <exception cref="OverflowException">
	/// The resulting <see cref="Date" /> is less than <see cref="MinValue" />.
	/// </exception>
#pragma warning disable CA2225 // Operator overloads have named alternates (Decrement)
	public static Date operator --(Date date)
#pragma warning restore CA2225
	{
		return AddSmallNegativeDays(date, number: -1);
	}

	/// <summary>
	/// Increments a specified date by one day.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> to increment.
	/// </param>
	/// <returns>
	/// The <see cref="Date" /> that represents the day after the specified <paramref name="date" />.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// <paramref name="date" /> is <see cref="Empty" />.
	/// </exception>
	/// <exception cref="OverflowException">
	/// The resulting <see cref="Date" /> is greater than <see cref="MaxValue" />.
	/// </exception>
#pragma warning disable CA2225 // Operator overloads have named alternates (Increment)
	public static Date operator ++(Date date)
#pragma warning restore CA2225
	{
		return AddSmallPositiveDays(date, number: 1);
	}

	/// <summary>
	/// Subtracts one specified <see cref="Date" /> from another, yielding the number of days between them.
	/// </summary>
	/// <param name="date1">
	/// The <see cref="Date" /> from which to subtract.
	/// </param>
	/// <param name="date2">
	/// The <see cref="Date" /> to subtract.
	/// </param>
	/// <returns>
	/// <para>
	/// A signed integer whose magnitude equals the number of days between <paramref name="date1" />
	/// and <paramref name="date2" />, and whose sign indicates their relative order.
	/// </para>
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>Less than zero</term>
	/// <description>
	/// <paramref name="date1" /> is less (earlier) than <paramref name="date2" />.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Zero</term>
	/// <description>
	/// <paramref name="date1" /> is equal to <paramref name="date2" />.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Greater than zero</term>
	/// <description>
	/// <paramref name="date1" /> is greater (later) than <paramref name="date2" />.
	/// </description>
	/// </item>
	/// </list>
	/// </returns>
	/// <remarks>
	/// This operator is equivalent to the <see cref="Subtract" /> method.
	/// </remarks>
	public static Int32 operator -(Date date1, Date date2)
	{
		return Subtract(date1, date2);
	}
}
