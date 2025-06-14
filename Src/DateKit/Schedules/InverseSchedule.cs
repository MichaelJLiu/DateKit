using System;
using System.Collections.Generic;

namespace DateKit.Schedules;

/// <summary>
/// Represents the inverse of another schedule.
/// </summary>
/// <remarks>
/// An <see cref="InverseSchedule" /> contains every date that is not in its <see cref="BaseSchedule" />,
/// and vice versa.
/// </remarks>
/// <threadsafety static="true" instance="true" />
public sealed class InverseSchedule : Schedule
{
	/// <summary>
	/// Initializes a new instance of the <see cref="InverseSchedule" /> class.
	/// </summary>
	/// <param name="baseSchedule">
	/// The <see cref="Schedule" /> to invert.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="baseSchedule" /> is <see langword="null" />.
	/// </exception>
	public InverseSchedule(Schedule baseSchedule)
	{
		ThrowHelper.ThrowIfArgumentIsNull(baseSchedule);
		this.BaseSchedule = baseSchedule;
	}

	/// <summary>
	/// Gets the base schedule.
	/// </summary>
	/// <value>
	/// The <see cref="Schedule" /> to invert.
	/// </value>
	public Schedule BaseSchedule { get; }

	/// <inheritdoc />
	protected internal override Boolean ContainsCore(Date date)
	{
		return !this.BaseSchedule.ContainsCore(date);
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateBackwardFromCore(Date date)
	{
		using (IEnumerator<Date> enumerator = this.BaseSchedule.EnumerateBackwardFromCore(date).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				while (true)
				{
					Date baseDate = enumerator.Current;

					while (date > baseDate)
					{
						yield return date;
						--date;
					}

					if (enumerator.MoveNext())
						--date; // Skip baseDate.
					else
						break;
				}
			}
			else
			{
				yield return date;
			}
		}

		while (date > Date.MinValue)
		{
			--date;
			yield return date;
		}
	}

	/// <inheritdoc />
	protected internal override IEnumerable<Date> EnumerateForwardFromCore(Date date)
	{
		using (IEnumerator<Date> enumerator = this.BaseSchedule.EnumerateForwardFromCore(date).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				while (true)
				{
					Date baseDate = enumerator.Current;

					while (date < baseDate)
					{
						yield return date;
						++date;
					}

					if (enumerator.MoveNext())
						++date; // Skip baseDate.
					else
						break;
				}
			}
			else
			{
				yield return date;
			}
		}

		while (date < Date.MaxValue)
		{
			++date;
			yield return date;
		}
	}
}
