using System;

namespace DateKit;

partial struct Date
{
	/// <summary>
	/// Compares two <see cref="Date" /> instances and returns a signed integer that indicates their relative order.
	/// </summary>
	/// <param name="date1">
	/// The first <see cref="Date" /> to compare.
	/// </param>
	/// <param name="date2">
	/// The second <see cref="Date" /> to compare.
	/// </param>
	/// <returns>
	/// <para>
	/// A signed integer that indicates the relative order of <paramref name="date1" /> and <paramref name="date2" />.
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
	public static Int32 Compare(Date date1, Date date2)
	{
		return date1._packedValue - date2._packedValue;
	}

	/// <summary>
	/// Compares this instance to another specified <see cref="Date" />
	/// and returns a signed integer that indicates their relative order.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> to compare to this instance.
	/// </param>
	/// <returns>
	/// <para>
	/// A signed integer that indicates the relative order of this instance and <paramref name="date" />.
	/// </para>
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>Less than zero</term>
	/// <description>
	/// This instance is less (earlier) than <paramref name="date" />.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Zero</term>
	/// <description>
	/// This instance is equal to <paramref name="date" />.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Greater than zero</term>
	/// <description>
	/// This instance is greater (later) than <paramref name="date" />.
	/// </description>
	/// </item>
	/// </list>
	/// </returns>
	public Int32 CompareTo(Date date)
	{
		return Compare(this, date);
	}

	/// <summary>
	/// Compares this instance to a specified object, which must be a boxed <see cref="Date" />,
	/// and returns a signed integer that indicates their relative order.
	/// </summary>
	/// <param name="value">
	/// The boxed <see cref="Date" /> to compare to this instance, or <see langword="null" />.
	/// </param>
	/// <returns>
	/// <para>
	/// A signed integer that indicates the relative order of this instance and <paramref name="value" />.
	/// </para>
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>Less than zero</term>
	/// <description>
	/// This instance is less (earlier) than <paramref name="value" />.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Zero</term>
	/// <description>
	/// This instance is equal to <paramref name="value" />.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Greater than zero</term>
	/// <description>
	/// This instance is greater (later) than <paramref name="value" />,
	/// or <paramref name="value" /> is <see langword="null" />.
	/// </description>
	/// </item>
	/// </list>
	/// </returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="value" /> is not a boxed <see cref="Date" />.
	/// </exception>
	/// <remarks>
	/// Any instance of <see cref="Date" /> is considered greater than <see langword="null" />.
	/// </remarks>
	public Int32 CompareTo(Object? value)
	{
		return
			value switch
			{
				null => 1,
				Date date => Compare(this, date),
				_ => throw new ArgumentException("Object must be of type Date.", nameof(value)),
			};
	}

	/// <overloads>
	/// Determines whether two <see cref="Date" /> instances, or a <see cref="Date" /> and an <see cref="Object" />,
	/// are equal.
	/// </overloads>
	/// <summary>
	/// Determines whether this instance is equal to a specified <see cref="Date" />.
	/// </summary>
	/// <param name="date">
	/// The <see cref="Date" /> to compare to this instance.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="date" /> represents the same date as this instance;
	/// otherwise, <see langword="false" />.
	/// </returns>
	public Boolean Equals(Date date)
	{
		return Equals(this, date);
	}

	/// <summary>
	/// Determines whether this instance is equal to a specified <see cref="Object" />.
	/// </summary>
	/// <param name="obj">
	/// The <see cref="Object" /> to compare to this instance.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="obj" /> is a <see cref="Date" /> that represents the same date
	/// as this instance; otherwise, <see langword="false" />.
	/// </returns>
	public override Boolean Equals(Object? obj)
	{
		return obj is Date date && Equals(this, date);
	}

	/// <summary>
	/// Determines whether two specified <see cref="Date" /> instances are equal.
	/// </summary>
	/// <param name="date1">
	/// The first <see cref="Date" /> to compare.
	/// </param>
	/// <param name="date2">
	/// The second <see cref="Date" /> to compare.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="date1" /> and <paramref name="date2" /> represent the same date;
	/// otherwise, <see langword="false" />.
	/// </returns>
	public static Boolean Equals(Date date1, Date date2)
	{
		return date1._packedValue == date2._packedValue;
	}

	/// <summary>
	/// Gets the hash code for this instance.
	/// </summary>
	/// <returns>
	/// A signed integer that represents the hash code for this instance.
	/// </returns>
	public override Int32 GetHashCode()
	{
		return _packedValue;
	}

	/// <summary>
	/// Determines whether two specified <see cref="Date" /> instances are equal.
	/// </summary>
	/// <param name="date1">
	/// The first <see cref="Date" /> to compare.
	/// </param>
	/// <param name="date2">
	/// The second <see cref="Date" /> to compare.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="date1" /> and <paramref name="date2" /> represent the same date;
	/// otherwise, <see langword="false" />.
	/// </returns>
	/// <remarks>
	/// This operator is equivalent to the <see cref="Equals(Date, Date)" /> method.
	/// </remarks>
	public static Boolean operator ==(Date date1, Date date2)
	{
		return Equals(date1, date2);
	}

	/// <summary>
	/// Determines whether two specified <see cref="Date" /> instances are unequal.
	/// </summary>
	/// <param name="date1">
	/// The first <see cref="Date" /> to compare.
	/// </param>
	/// <param name="date2">
	/// The second <see cref="Date" /> to compare.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="date1" /> and <paramref name="date2" /> represent different dates;
	/// otherwise, <see langword="false" />.
	/// </returns>
	public static Boolean operator !=(Date date1, Date date2)
	{
		return !Equals(date1, date2);
	}

	/// <summary>
	/// Determines whether one specified <see cref="Date" /> is less than another.
	/// </summary>
	/// <param name="date1">
	/// The first <see cref="Date" /> to compare.
	/// </param>
	/// <param name="date2">
	/// The second <see cref="Date" /> to compare.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="date1" /> is less (earlier) than
	/// <paramref name="date2" />; otherwise, <see langword="false" />.
	/// </returns>
	public static Boolean operator <(Date date1, Date date2)
	{
		return Compare(date1, date2) < 0;
	}

	/// <summary>
	/// Determines whether one specified <see cref="Date" /> is less than or equal to another.
	/// </summary>
	/// <param name="date1">
	/// The first <see cref="Date" /> to compare.
	/// </param>
	/// <param name="date2">
	/// The second <see cref="Date" /> to compare.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="date1" /> is less (earlier) than or equal to
	/// <paramref name="date2" />; otherwise, <see langword="false" />.
	/// </returns>
	public static Boolean operator <=(Date date1, Date date2)
	{
		return Compare(date1, date2) <= 0;
	}

	/// <summary>
	/// Determines whether one specified <see cref="Date" /> is greater than another.
	/// </summary>
	/// <param name="date1">
	/// The first <see cref="Date" /> to compare.
	/// </param>
	/// <param name="date2">
	/// The second <see cref="Date" /> to compare.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="date1" /> is greater (later) than
	/// <paramref name="date2" />; otherwise, <see langword="false" />.
	/// </returns>
	public static Boolean operator >(Date date1, Date date2)
	{
		return Compare(date1, date2) > 0;
	}

	/// <summary>
	/// Determines whether one specified <see cref="Date" /> is greater than or equal to another.
	/// </summary>
	/// <param name="date1">
	/// The first <see cref="Date" /> to compare.
	/// </param>
	/// <param name="date2">
	/// The second <see cref="Date" /> to compare.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="date1" /> is greater (later) than or equal to
	/// <paramref name="date2" />; otherwise, <see langword="false" />.
	/// </returns>
	public static Boolean operator >=(Date date1, Date date2)
	{
		return Compare(date1, date2) >= 0;
	}
}
