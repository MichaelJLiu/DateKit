using System;

namespace DateKit.Schedules;

/// <summary>
/// Specifies options for a <see cref="GregorianEasterRecurrence" />.
/// </summary>
public class GregorianEasterRecurrenceOptions : AnnualRecurrenceOptions
{
	internal static readonly GregorianEasterRecurrenceOptions Default = new();

	private Int32 _offset;

	/// <summary>
	/// Gets or sets the offset in days of the event from Easter.
	/// </summary>
	/// <value>
	/// An integer between -80 and 250 that specifies the number of days between Easter and the event.
	/// The default is zero.
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The value specified when setting the property is less than -80 or greater than 250.
	/// </exception>
	public Int32 Offset
	{
		get => _offset;
		set
		{
			const Int32 minOffset = -80; // days between March 22 and January 1
			const Int32 maxOffset = 250; // days between April 25 and December 31
			if (value is < minOffset or > maxOffset)
				ThrowHelper.ThrowArgumentOutOfRangeException(value, ExceptionArgument.value);
			_offset = value;
		}
	}
}
