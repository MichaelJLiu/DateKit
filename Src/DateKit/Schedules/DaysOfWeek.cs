using System;

namespace DateKit.Schedules;

/// <summary>
/// Specifies the days of the week on which a <see cref="DaysOfWeekRecurrence" /> occurs.
/// </summary>
[Flags]
public enum DaysOfWeek
{
	/// <summary>
	/// The recurrence occurs every week on Sunday.
	/// </summary>
	Sunday = 1 << DayOfWeek.Sunday,

	/// <summary>
	/// The recurrence occurs every week on Monday.
	/// </summary>
	Monday = 1 << DayOfWeek.Monday,

	/// <summary>
	/// The recurrence occurs every week on Tuesday.
	/// </summary>
	Tuesday = 1 << DayOfWeek.Tuesday,

	/// <summary>
	/// The recurrence occurs every week on Wednesday.
	/// </summary>
	Wednesday = 1 << DayOfWeek.Wednesday,

	/// <summary>
	/// The recurrence occurs every week on Thursday.
	/// </summary>
	Thursday = 1 << DayOfWeek.Thursday,

	/// <summary>
	/// The recurrence occurs every week on Friday.
	/// </summary>
	Friday = 1 << DayOfWeek.Friday,

	/// <summary>
	/// The recurrence occurs every week on Saturday.
	/// </summary>
	Saturday = 1 << DayOfWeek.Saturday,
}

internal static class DaysOfWeekExtensions
{
	public static Boolean Contains(this DaysOfWeek daysOfWeek, DayOfWeek dayOfWeek)
	{
		return ((Int32)daysOfWeek & (1 << (Int32)dayOfWeek)) != 0;
	}
}
