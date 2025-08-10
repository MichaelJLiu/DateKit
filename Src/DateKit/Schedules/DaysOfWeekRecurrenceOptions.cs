using System;

namespace DateKit.Schedules;

/// <summary>
/// Specifies options for a <see cref="DaysOfWeekRecurrence" />.
/// </summary>
public class DaysOfWeekRecurrenceOptions
{
	internal static readonly DaysOfWeekRecurrenceOptions Default = new();

	private Date _startDate = Date.MinValue;
	private Date _endDate = Date.MaxValue;

	/// <summary>
	/// Gets or sets the date on which the recurrence starts.
	/// </summary>
	/// <value>
	/// The <see cref="Date" /> on which the recurrence starts. The default is <see cref="Date.MinValue" />.
	/// </value>
	/// <exception cref="ArgumentException">
	/// The value specified when setting the property is <see cref="Date.Empty" />.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The value specified when setting the property is later than six days before <see cref="EndDate" />.
	/// </exception>
	public Date StartDate
	{
		get => _startDate;
		set
		{
			ThrowHelper.ThrowIfDateArgumentIsEmpty(value, ExceptionArgument.value);

			Date maxStartDate = Date.AddSmallNegativeDays(this.EndDate, 1 - Date.DaysPerWeek);

			if (value > maxStartDate)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value,
					$"The value cannot be later than {maxStartDate:o} (six days before {nameof(this.EndDate)}).");
			}

			_startDate = value;
		}
	}

	/// <summary>
	/// Gets or sets the date on which the recurrence ends.
	/// </summary>
	/// <value>
	/// The <see cref="Date" /> on which the recurrence ends. The default is <see cref="Date.MaxValue" />.
	/// </value>
	/// <exception cref="ArgumentException">
	/// The value specified when setting the property is <see cref="Date.Empty" />.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The value specified when setting the property is earlier than six days after <see cref="StartDate" />.
	/// </exception>
	public Date EndDate
	{
		get => _endDate;
		set
		{
			ThrowHelper.ThrowIfDateArgumentIsEmpty(value, ExceptionArgument.value);

			Date minEndDate = Date.AddSmallPositiveDays(this.StartDate, Date.DaysPerWeek - 1);

			if (value < minEndDate)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value,
					$"The value cannot be earlier than {minEndDate:o} (six days after {nameof(this.StartDate)}).");
			}

			_endDate = value;
		}
	}
}
