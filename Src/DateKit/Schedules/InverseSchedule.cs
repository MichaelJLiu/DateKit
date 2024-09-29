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
public class InverseSchedule : Schedule
{
	private readonly Schedule _baseSchedule;

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
		_baseSchedule = baseSchedule;
	}

	/// <summary>
	/// Gets the base schedule.
	/// </summary>
	/// <value>
	/// The <see cref="Schedule" /> to invert.
	/// </value>
	public Schedule BaseSchedule => _baseSchedule;

	/// <inheritdoc />
	public override Boolean Contains(Date date)
	{
		return !_baseSchedule.Contains(date) && date != Date.Empty;
	}

	/// <inheritdoc />
	public override IEnumerable<Date> EnumerateBackwardFrom(Date date)
	{
		ThrowHelper.ThrowIfDateArgumentIsEmpty(date, ExceptionArgument.date);
		return this.EnumerateBackwardFromIterator(date);
	}

	private IEnumerable<Date> EnumerateBackwardFromIterator(Date date)
	{
		using (IEnumerator<Date> enumerator = _baseSchedule.EnumerateBackwardFrom(date).GetEnumerator())
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
				yield return date;
		}

		while (date > Date.MinValue)
		{
			--date;
			yield return date;
		}
	}

	/// <inheritdoc />
	public override IEnumerable<Date> EnumerateForwardFrom(Date date)
	{
		ThrowHelper.ThrowIfDateArgumentIsEmpty(date, ExceptionArgument.date);
		return this.EnumerateForwardFromIterator(date);
	}

	private IEnumerable<Date> EnumerateForwardFromIterator(Date date)
	{
		using (IEnumerator<Date> enumerator = _baseSchedule.EnumerateForwardFrom(date).GetEnumerator())
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
				yield return date;
		}

		while (date < Date.MaxValue)
		{
			++date;
			yield return date;
		}
	}
}
