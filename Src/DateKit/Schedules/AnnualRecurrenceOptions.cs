using System;

namespace DateKit.Schedules;

/// <summary>
/// Specifies options for an <see cref="AnnualRecurrence" />.
/// </summary>
public class AnnualRecurrenceOptions
{
	private Int32 _startYear = Date.MinYear;
	private Int32 _endYear = Date.MaxYear;

	/// <summary>
	/// Gets or sets the year in which the recurrence starts.
	/// </summary>
	/// <value>
	/// An integer between <see cref="Date.MinYear" /> and <see cref="EndYear" /> that specifies
	/// the year in which the recurrence starts. The default is <see cref="Date.MinYear" />.
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
					$"The value must be between {Date.MinYear} and {this.EndYear} ({nameof(this.EndYear)}).");
			}

			_startYear = value;
		}
	}

	/// <summary>
	/// Gets or sets the year in which the recurrence ends.
	/// </summary>
	/// <value>
	/// An integer between <see cref="StartYear" /> and <see cref="Date.MaxYear" /> that specifies
	/// the year in which the recurrence ends. The default is <see cref="Date.MaxYear" />.
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
					$"The value must be between {this.StartYear} ({nameof(this.StartYear)}) and {Date.MaxYear}.");
			}

			_endYear = value;
		}
	}
}
