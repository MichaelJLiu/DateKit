using System;

namespace DateKit.Schedules;

/// <summary>
/// Specifies options for an <see cref="AnnualDayOfWeekOfMonthRecurrence" />.
/// </summary>
public class AnnualDayOfWeekOfMonthRecurrenceOptions
{
	internal static readonly AnnualDayOfWeekOfMonthRecurrenceOptions Default = new();

	private Int32 _startYear = Date.MinYear;
	private Int32 _endYear = Date.MaxYear;

	/// <summary>
	/// Gets or sets the first year in which the event occurs.
	/// </summary>
	/// <value>
	/// An integer between <see cref="Date.MinYear" /> and <see cref="EndYear" /> that specifies the first year
	/// in which the event occurs. The default is <see cref="Date.MinYear" />.
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The value specified when setting the property is less than <see cref="Date.MinYear" />
	/// or greater than <see cref="EndYear" />.
	/// </exception>
	public Int32 StartYear
	{
		get => _startYear;
		set
		{
			if (value < Date.MinYear || value > this.EndYear)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value,
					$"{nameof(this.StartYear)} must be between {Date.MinYear} and {this.EndYear} (the value of {nameof(this.EndYear)}).");
			}

			_startYear = value;
		}
	}

	/// <summary>
	/// Gets or sets the last year in which the event occurs.
	/// </summary>
	/// <value>
	/// An integer between <see cref="StartYear" /> and <see cref="Date.MaxYear" /> that specifies the last year
	/// in which the event occurs. The default is <see cref="Date.MaxYear" />.
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The value specified when setting the property is less than <see cref="StartYear" />
	/// or greater than <see cref="Date.MaxYear" />.
	/// </exception>
	public Int32 EndYear
	{
		get => _endYear;
		set
		{
			if (value < this.StartYear || value > Date.MaxYear)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value,
					$"{nameof(this.EndYear)} must be between {this.StartYear} (the value of {nameof(this.StartYear)}) and {Date.MaxYear}.");
			}

			_endYear = value;
		}
	}
}
