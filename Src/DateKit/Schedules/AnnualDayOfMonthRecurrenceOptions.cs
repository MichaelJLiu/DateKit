namespace DateKit.Schedules;

/// <summary>
/// Specifies options for an <see cref="AnnualDayOfMonthRecurrence" />.
/// </summary>
public class AnnualDayOfMonthRecurrenceOptions : AnnualRecurrenceOptions
{
	internal new static readonly AnnualDayOfMonthRecurrenceOptions Default = new();

	/// <summary>
	/// Gets or sets the adjustments to make to the <see cref="AnnualDayOfMonthRecurrence.Day" />
	/// of the <see cref="AnnualDayOfMonthRecurrence" /> when it falls on particular days of the week.
	/// </summary>
	/// <value>
	/// A <see cref="Schedules.DayOfWeekAdjustments" /> instance, or <see langword="null" /> to make no adjustments.
	/// The default is <see langword="null" />.
	/// </value>
	public DayOfWeekAdjustments? DayOfWeekAdjustments { get; set; }
}
